using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using Controls = System.Windows.Controls;
using Media = System.Windows.Media;
using WPF = System.Windows;

namespace CustomObjectWPF.Model
{
    #region Helpers

    /// <summary>
    /// Factorise l'effet néon partagé par tous les contrôles customs.
    /// Les DPs restent déclarés sur chaque contrôle (pour garder la syntaxe
    /// Neon="True" dans le XAML), mais le corps de l'effet n'est écrit qu'ici.
    /// NOTE : la cible doit être la couche visuelle de fond (pas le contrôle
    /// entier), sinon le texte devient flou (ClearType désactivé).
    /// </summary>
    public static class NeonHelper
    {
        public static void ApplyEffect(UIElement target, bool neon, Color color, double radius)
        {
            if (target == null) return;

            if (!neon)
            {
                target.Effect = null;
                return;
            }

            target.Effect = new DropShadowEffect
            {
                BlurRadius = radius,
                ShadowDepth = 0,
                Opacity = 1,
                Color = color
            };
        }
    }

    /// <summary>
    /// Convertit un double (espacement) en Thickness avec marge à gauche.
    /// Utilisé pour l'espacement case/label du CheckBoxCustom.
    /// </summary>
    public class DoubleToLeftMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double d = value is double v ? v : 0;
            return new Thickness(d, 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Renvoie le premier Brush non nul parmi les valeurs liées
    /// (ex : HoverBackground, sinon Background).
    /// Sert aux triggers survol/clic du ButtonCustom : si l'utilisateur n'a
    /// pas défini HoverBackground/PressedBackground, le bouton garde son
    /// Background au lieu de passer à une couleur par défaut imposée.
    /// </summary>
    public class FirstNonNullBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (object v in values)
            {
                if (v is Brush b) return b;
            }
            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    #endregion

    #region ButtonCustom

    public class ButtonCustom : Controls.Button
    {
        #region Dependency Properties

        public static readonly DependencyProperty NeonProperty =
            DependencyProperty.Register(
                nameof(Neon),
                typeof(bool),
                typeof(ButtonCustom),
                new PropertyMetadata(false, OnNeonPropertyChanged));

        public static readonly DependencyProperty NeonRadiusProperty =
            DependencyProperty.Register(
                nameof(NeonRadius),
                typeof(double),
                typeof(ButtonCustom),
                new PropertyMetadata(20.0, OnNeonPropertyChanged));

        public static readonly DependencyProperty NeonColorProperty =
            DependencyProperty.Register(
                nameof(NeonColor),
                typeof(Media.Color),
                typeof(ButtonCustom),
                new PropertyMetadata(Media.Colors.Cyan, OnNeonPropertyChanged));

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(ButtonCustom),
                new PropertyMetadata(new CornerRadius(15)));

        // FIX (2026-11-05) : défaut null, pas gris clair imposé.
        // Avant, HoverBackground valait #E4E4E4 et PressedBackground #A8A8A8
        // par défaut : dès qu'on survolait un bouton SANS avoir défini ces
        // propriétés, le trigger mettait ce gris clair — sur les boutons
        // sombres de Samuel (Background #FF272727, texte blanc), le texte
        // devenait illisible. Avec null, le MultiBinding du template
        // retombe sur Background : le bouton garde son fond au survol/clic
        // tant qu'on n'a rien demandé d'autre (c'est aussi ce que dit la
        // Vitrine : « Si tu ne le mets pas, le bouton garde son Background »).
        public static readonly DependencyProperty HoverBackgroundProperty =
            DependencyProperty.Register(
                nameof(HoverBackground),
                typeof(Brush),
                typeof(ButtonCustom),
                new PropertyMetadata(null));

        public static readonly DependencyProperty PressedBackgroundProperty =
            DependencyProperty.Register(
                nameof(PressedBackground),
                typeof(Brush),
                typeof(ButtonCustom),
                new PropertyMetadata(null));

        // FIX (2026-11-08) : « bouton toujours pressé » suivi par nous-mêmes.
        // ButtonBase.IsPressed n'est PAS « bouton de souris enfoncé » : WPF le
        // met à false dès que la souris sort du bouton, même si on tient
        // toujours le clic (c'est le comportement normal d'un Button : le
        // Click ne se déclenche que si on relâche AU-DESSUS du bouton).
        // Donc le trigger sur IsPressed se désactivait quand on glissait
        // hors du bouton, et le fond retombait sur Background. IsPressedHeld,
        // lui, passe à true au PreviewMouseLeftButtonDown et ne retombe à
        // false qu'au relâché (ou perte de capture) : il reste vrai tant
        // qu'on tient le clic, souris dessus ou non. DP en lecture seule :
        // seul le contrôle lui-même peut le changer.
        private static readonly DependencyPropertyKey IsPressedHeldPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(IsPressedHeld),
                typeof(bool),
                typeof(ButtonCustom),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsPressedHeldProperty =
            IsPressedHeldPropertyKey.DependencyProperty;

        #endregion

        #region Propriétés

        public bool Neon
        {
            get => (bool)GetValue(NeonProperty);
            set => SetValue(NeonProperty, value);
        }

        public Media.Color NeonColor
        {
            get => (Media.Color)GetValue(NeonColorProperty);
            set => SetValue(NeonColorProperty, value);
        }

        public double NeonRadius
        {
            get => (double)GetValue(NeonRadiusProperty);
            set => SetValue(NeonRadiusProperty, value);
        }

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        /// <summary>
        /// Fond affiché au survol de la souris.
        /// null (défaut) = garde le Background actuel.
        /// </summary>
        public Brush HoverBackground
        {
            get => (Brush)GetValue(HoverBackgroundProperty);
            set => SetValue(HoverBackgroundProperty, value);
        }

        /// <summary>
        /// Fond affiché pendant le clic.
        /// null (défaut) = garde HoverBackground si défini, sinon Background.
        /// </summary>
        public Brush PressedBackground
        {
            get => (Brush)GetValue(PressedBackgroundProperty);
            set => SetValue(PressedBackgroundProperty, value);
        }

        /// <summary>
        /// Vrai tant que le bouton gauche de la souris est tenu enfoncé
        /// après un clic sur ce bouton, même si la souris a glissé hors
        /// du bouton. Faux au relâché ou à la perte de capture.
        /// </summary>
        public bool IsPressedHeld
        {
            get => (bool)GetValue(IsPressedHeldProperty);
        }

        #endregion

        #region Constructeur

        public ButtonCustom()
        {
            // FIX (2026-11-07) : défauts posés avec SetCurrentValue, pas avec
            // une affectation directe. Une affectation (Background = ...) crée
            // une VALEUR LOCALE, qui a priorité sur les setters d'un Style
            // (doc Microsoft : « Dependency property value precedence » —
            // valeur locale = rang 3, setter de Style = rang 8). Conséquence :
            // un Style implicite ou explicite qui définissait Background,
            // Foreground, BorderThickness ou FontWeight était silencieusement
            // ignoré — le bouton restait gris clair / texte noir, et ça ne se
            // voyait pas dans InfiniteMouse ni dans la Vitrine parce que les
            // deux mettent Background en attribut direct (valeur locale aussi,
            // posée après le constructeur, donc elle gagne pareil). Avec
            // SetCurrentValue, les défauts ci-dessous s'appliquent quand rien
            // d'autre ne les définit, mais un Style, un trigger ou un binding
            // reprend le dessus normalement. Les 4 autres contrôles ne posent
            // pas de défauts dans leur constructeur, rien à changer pour eux.
            SetCurrentValue(BackgroundProperty, Media.Brushes.LightGray);
            SetCurrentValue(ForegroundProperty, Media.Brushes.Black);

            SetCurrentValue(BorderThicknessProperty, new Thickness(1));

            SetCurrentValue(FontWeightProperty, FontWeights.SemiBold);

            // FIX : le scale du clic pivote autour de ce point. (0.5, 0.5) = centre.
            // Sans ça, le défaut (0,0) = coin haut-gauche et le bouton "saute" à chaque pression.
            RenderTransformOrigin = new Point(0.5, 0.5);

            RefreshNeon(); // fallback sur le contrôle tant que le template n'est pas appliqué

            Template = BuildTemplate();

            // Note : l'ancien fondu au survol (Opacity 0.85) est remplacé par
            // HoverBackground, plus propre. Le scale au clic est conservé.
            MouseLeave += ButtonCustom_MouseLeave;
            PreviewMouseLeftButtonDown += ButtonCustom_Down;
            PreviewMouseLeftButtonUp += ButtonCustom_Up;
            LostMouseCapture += ButtonCustom_LostMouseCapture;
        }

        #endregion

        #region Callbacks

        private static void OnNeonPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ButtonCustom button)
            {
                button.RefreshNeon();
            }
        }

        #endregion

        #region Néon (lueur derrière le texte)

        // La lueur est appliquée sur le Border interne ("glowBorder"), PAS sur le
        // contrôle entier : le texte reste net (sinon ClearType désactivé + flou).
        private Border _glowBorder;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _glowBorder = Template.FindName("glowBorder", this) as Border;
            RefreshNeon();
        }

        private void RefreshNeon()
        {
            if (_glowBorder != null)
            {
                NeonHelper.ApplyEffect(_glowBorder, Neon, NeonColor, NeonRadius);
                Effect = null; // pas de double lueur : elle est sur la couche interne
            }
            else
            {
                // Template pas encore appliqué (constructeur) : fallback temporaire
                NeonHelper.ApplyEffect(this, Neon, NeonColor, NeonRadius);
            }
        }

        #endregion

        #region Événements Souris

        private void ButtonCustom_MouseLeave(object sender, MouseEventArgs e)
        {
            // FIX : si le clic est relâché hors du bouton, PreviewMouseLeftButtonUp
            // ne passe jamais par lui. Sans cette ligne, le scale resterait à 0.97
            // ("écrasé") jusqu'au prochain clic.
            // MAIS : tant qu'on tient le clic (IsPressedHeld), on ne touche à
            // rien — le bouton est toujours pressé, juste la souris est sortie,
            // le scale 0.97 et le fond PressedBackground doivent rester.
            if (!IsPressedHeld)
            {
                RenderTransform = null;
            }
        }

        private void ButtonCustom_Down(object sender, MouseButtonEventArgs e)
        {
            // Le bouton reste « tenu » même si la souris glisse hors du
            // bouton ensuite (voir IsPressedHeld) : c'est ce flag, et non
            // IsPressed de WPF, qui pilote le fond pressé hors du bouton.
            SetValue(IsPressedHeldPropertyKey, true);
            RenderTransform = new ScaleTransform(0.97, 0.97);
        }

        private void ButtonCustom_Up(object sender, MouseButtonEventArgs e)
        {
            SetValue(IsPressedHeldPropertyKey, false);
            RenderTransform = null;
        }

        private void ButtonCustom_LostMouseCapture(object sender, MouseEventArgs e)
        {
            // Sécurité : si la capture est perdue sans relâché propre
            // (fenêtre désactivée, autre contrôle qui capture...), on ne
            // reste jamais coincé en état pressé.
            SetValue(IsPressedHeldPropertyKey, false);
            RenderTransform = null;
        }

        #endregion

        #region Template

        private static Controls.ControlTemplate BuildTemplate()
        {
            // Structure : un Grid avec DEUX couches superposées.
            // - "glowBorder" : le fond du bouton, qui porte la lueur néon.
            // - le ContentPresenter : le texte, SANS effet -> reste net et lisible.
            FrameworkElementFactory grid = new(typeof(Controls.Grid));

            FrameworkElementFactory glowBorder = new(typeof(Controls.Border));
            glowBorder.Name = "glowBorder";

            glowBorder.SetBinding(
                Controls.Border.BackgroundProperty,
                new WPF.Data.Binding(nameof(Background))
                {
                    RelativeSource = new WPF.Data.RelativeSource(WPF.Data.RelativeSourceMode.TemplatedParent)
                });

            glowBorder.SetBinding(
                Controls.Border.CornerRadiusProperty,
                new WPF.Data.Binding(nameof(CornerRadius))
                {
                    RelativeSource = new WPF.Data.RelativeSource(WPF.Data.RelativeSourceMode.TemplatedParent)
                });

            glowBorder.SetBinding(
                Controls.Border.BorderBrushProperty,
                new WPF.Data.Binding(nameof(BorderBrush))
                {
                    RelativeSource = new WPF.Data.RelativeSource(WPF.Data.RelativeSourceMode.TemplatedParent)
                });

            glowBorder.SetBinding(
                Controls.Border.BorderThicknessProperty,
                new WPF.Data.Binding(nameof(BorderThickness))
                {
                    RelativeSource = new WPF.Data.RelativeSource(WPF.Data.RelativeSourceMode.TemplatedParent)
                });

            FrameworkElementFactory presenter = new(typeof(Controls.ContentPresenter));

            presenter.SetBinding(
                Controls.ContentPresenter.ContentProperty,
                new WPF.Data.Binding(nameof(Content))
                {
                    RelativeSource = new WPF.Data.RelativeSource(WPF.Data.RelativeSourceMode.TemplatedParent)
                });

            presenter.SetValue(
                Controls.ContentPresenter.HorizontalAlignmentProperty,
                WPF.HorizontalAlignment.Center);

            presenter.SetValue(
                Controls.ContentPresenter.VerticalAlignmentProperty,
                VerticalAlignment.Center);

            grid.AppendChild(glowBorder);
            grid.AppendChild(presenter);

            Controls.ControlTemplate template = new(typeof(ButtonCustom));
            template.VisualTree = grid;

            // Survol : fond HoverBackground, ou Background si non défini
            // (FirstNonNullBrushConverter : premier Brush non nul).
            Trigger hoverTrigger = new()
            {
                Property = UIElement.IsMouseOverProperty,
                Value = true
            };
            MultiBinding hoverBinding = new() { Converter = new FirstNonNullBrushConverter() };
            hoverBinding.Bindings.Add(new WPF.Data.Binding(nameof(HoverBackground))
            {
                RelativeSource = new WPF.Data.RelativeSource(WPF.Data.RelativeSourceMode.TemplatedParent)
            });
            hoverBinding.Bindings.Add(new WPF.Data.Binding(nameof(Background))
            {
                RelativeSource = new WPF.Data.RelativeSource(WPF.Data.RelativeSourceMode.TemplatedParent)
            });
            hoverTrigger.Setters.Add(new Setter
            {
                Property = Controls.Border.BackgroundProperty,
                TargetName = "glowBorder",
                Value = hoverBinding
            });
            template.Triggers.Add(hoverTrigger);

            // Clic : fond PressedBackground (ajouté après = prioritaire sur le survol),
            // ou HoverBackground si défini, sinon Background.
            Trigger pressedTrigger = new()
            {
                Property = Controls.Primitives.ButtonBase.IsPressedProperty,
                Value = true
            };
            MultiBinding pressedBinding = new() { Converter = new FirstNonNullBrushConverter() };
            pressedBinding.Bindings.Add(new WPF.Data.Binding(nameof(PressedBackground))
            {
                RelativeSource = new WPF.Data.RelativeSource(WPF.Data.RelativeSourceMode.TemplatedParent)
            });
            pressedBinding.Bindings.Add(new WPF.Data.Binding(nameof(HoverBackground))
            {
                RelativeSource = new WPF.Data.RelativeSource(WPF.Data.RelativeSourceMode.TemplatedParent)
            });
            pressedBinding.Bindings.Add(new WPF.Data.Binding(nameof(Background))
            {
                RelativeSource = new WPF.Data.RelativeSource(WPF.Data.RelativeSourceMode.TemplatedParent)
            });
            pressedTrigger.Setters.Add(new Setter
            {
                Property = Controls.Border.BackgroundProperty,
                TargetName = "glowBorder",
                Value = pressedBinding
            });
            template.Triggers.Add(pressedTrigger);

            // Clic tenu hors du bouton : IsPressed (WPF) retombe à false dès
            // que la souris sort, mais IsPressedHeld reste vrai tant qu'on
            // tient le clic. Ajouté après = prioritaire, même fond pressé.
            // Le trigger IsPressed ci-dessus est gardé : c'est lui qui couvre
            // le clic au clavier (Espace), où la souris n'est pas enfoncée.
            Trigger pressedHeldTrigger = new()
            {
                Property = IsPressedHeldProperty,
                Value = true
            };
            pressedHeldTrigger.Setters.Add(new Setter
            {
                Property = Controls.Border.BackgroundProperty,
                TargetName = "glowBorder",
                Value = pressedBinding
            });
            template.Triggers.Add(pressedHeldTrigger);

            return template;
        }

        #endregion
    }

    #endregion

    #region CheckBoxCustom

    public class CheckBoxCustom : Controls.CheckBox
    {
        #region Dependency Properties

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(CheckBoxCustom),
                new PropertyMetadata(new CornerRadius(0)));

        public static readonly DependencyProperty BorderHoverProperty =
            DependencyProperty.Register(
                nameof(BorderHover),
                typeof(Brush),
                typeof(CheckBoxCustom),
                new PropertyMetadata(new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#FF2997DE"))));

        public static readonly DependencyProperty CheckColorProperty =
            DependencyProperty.Register(
                nameof(CheckColor),
                typeof(Brush),
                typeof(CheckBoxCustom),
                new PropertyMetadata(Media.Brushes.Black));

        /// <summary>
        /// Taille (côté, en px) de la case. Le crochet s'adapte automatiquement.
        /// </summary>
        public static readonly DependencyProperty BoxSizeProperty =
            DependencyProperty.Register(
                nameof(BoxSize),
                typeof(double),
                typeof(CheckBoxCustom),
                new PropertyMetadata(14.0));

        /// <summary>
        /// Espacement horizontal (en px) entre la case et le libellé.
        /// </summary>
        public static readonly DependencyProperty BoxSpacingProperty =
            DependencyProperty.Register(
                nameof(BoxSpacing),
                typeof(double),
                typeof(CheckBoxCustom),
                new PropertyMetadata(5.0));

        public static readonly DependencyProperty NeonProperty =
            DependencyProperty.Register(
                nameof(Neon),
                typeof(bool),
                typeof(CheckBoxCustom),
                new PropertyMetadata(false, OnNeonPropertyChanged));

        public static readonly DependencyProperty NeonRadiusProperty =
            DependencyProperty.Register(
                nameof(NeonRadius),
                typeof(double),
                typeof(CheckBoxCustom),
                new PropertyMetadata(20.0, OnNeonPropertyChanged));

        public static readonly DependencyProperty NeonColorProperty =
            DependencyProperty.Register(
                nameof(NeonColor),
                typeof(Media.Color),
                typeof(CheckBoxCustom),
                new PropertyMetadata(Media.Colors.Cyan, OnNeonPropertyChanged));

        #endregion

        #region Propriétés

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public Brush BorderHover
        {
            get => (Brush)GetValue(BorderHoverProperty);
            set => SetValue(BorderHoverProperty, value);
        }

        public Brush CheckColor
        {
            get => (Brush)GetValue(CheckColorProperty);
            set => SetValue(CheckColorProperty, value);
        }

        public double BoxSize
        {
            get => (double)GetValue(BoxSizeProperty);
            set => SetValue(BoxSizeProperty, value);
        }

        public double BoxSpacing
        {
            get => (double)GetValue(BoxSpacingProperty);
            set => SetValue(BoxSpacingProperty, value);
        }

        public bool Neon
        {
            get => (bool)GetValue(NeonProperty);
            set => SetValue(NeonProperty, value);
        }

        public Media.Color NeonColor
        {
            get => (Media.Color)GetValue(NeonColorProperty);
            set => SetValue(NeonColorProperty, value);
        }

        public double NeonRadius
        {
            get => (double)GetValue(NeonRadiusProperty);
            set => SetValue(NeonRadiusProperty, value);
        }

        #endregion

        #region Constructeur

        public CheckBoxCustom()
        {
            Template = BuildTemplate();
            RefreshNeon();
        }

        #endregion

        #region Callbacks

        private static void OnNeonPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CheckBoxCustom chk)
            {
                chk.RefreshNeon();
            }
        }

        // Note : contrairement à l'ancienne version, CheckColor et BorderHover
        // n'ont plus besoin de reconstruire le template à chaque changement :
        // ce sont maintenant des bindings dynamiques (voir BuildTemplate).

        #endregion

        #region Néon (lueur sur la case, libellé net)

        // La lueur est appliquée sur la case ("checkBoxBorder"), pas sur le contrôle
        // entier : le libellé à côté reste net et lisible.
        private Border _glowBox;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _glowBox = Template.FindName("checkBoxBorder", this) as Border;
            RefreshNeon();
        }

        private void RefreshNeon()
        {
            if (_glowBox != null)
            {
                NeonHelper.ApplyEffect(_glowBox, Neon, NeonColor, NeonRadius);
                Effect = null;
            }
            else
            {
                NeonHelper.ApplyEffect(this, Neon, NeonColor, NeonRadius);
            }
        }

        #endregion

        #region Template

        private Controls.ControlTemplate BuildTemplate()
        {
            FrameworkElementFactory panel = new(typeof(Controls.StackPanel));

            panel.SetValue(Controls.StackPanel.OrientationProperty, Controls.Orientation.Horizontal);
            panel.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

            FrameworkElementFactory border = new(typeof(Controls.Border));
            border.Name = "checkBoxBorder";

            // Taille de la case pilotée par BoxSize (binding dynamique)
            border.SetBinding(
                Controls.Border.WidthProperty,
                new WPF.Data.Binding(nameof(BoxSize))
                {
                    RelativeSource = new WPF.Data.RelativeSource(WPF.Data.RelativeSourceMode.TemplatedParent)
                });
            border.SetBinding(
                Controls.Border.HeightProperty,
                new WPF.Data.Binding(nameof(BoxSize))
                {
                    RelativeSource = new WPF.Data.RelativeSource(WPF.Data.RelativeSourceMode.TemplatedParent)
                });

            border.SetBinding(
                Controls.Border.BackgroundProperty,
                new WPF.Data.Binding(nameof(Background))
                {
                    RelativeSource = new WPF.Data.RelativeSource(WPF.Data.RelativeSourceMode.TemplatedParent)
                });

            border.SetBinding(
                Controls.Border.BorderBrushProperty,
                new WPF.Data.Binding(nameof(BorderBrush))
                {
                    RelativeSource = new WPF.Data.RelativeSource(WPF.Data.RelativeSourceMode.TemplatedParent)
                });

            border.SetBinding(
                Controls.Border.BorderThicknessProperty,
                new WPF.Data.Binding(nameof(BorderThickness))
                {
                    RelativeSource = new WPF.Data.RelativeSource(WPF.Data.RelativeSourceMode.TemplatedParent)
                });

            border.SetBinding(
                Controls.Border.CornerRadiusProperty,
                new WPF.Data.Binding(nameof(CornerRadius))
                {
                    RelativeSource = new WPF.Data.RelativeSource(WPF.Data.RelativeSourceMode.TemplatedParent)
                });

            // Crochet : binding dynamique sur CheckColor (pas de snapshot),
            // et pas de taille fixe -> il grandit avec BoxSize (Stretch Uniform).
            FrameworkElementFactory path = new(typeof(WPF.Shapes.Path));
            path.Name = "optionMark";

            path.SetValue(
                WPF.Shapes.Path.DataProperty,
                Geometry.Parse(
                    "F1 M 9.97498,1.22334L 4.6983,9.09834L 4.52164,9.09834L 0,5.19331L 1.27664,3.52165L 4.255,6.08833L 8.33331,1.52588e-005L 9.97498,1.22334 Z"));

            path.SetBinding(
                WPF.Shapes.Shape.FillProperty,
                new WPF.Data.Binding(nameof(CheckColor))
                {
                    RelativeSource = new WPF.Data.RelativeSource(WPF.Data.RelativeSourceMode.TemplatedParent)
                });

            path.SetValue(FrameworkElement.MarginProperty, new Thickness(2));
            path.SetValue(WPF.Shapes.Shape.StretchProperty, Stretch.Uniform);
            path.SetValue(FrameworkElement.HorizontalAlignmentProperty, WPF.HorizontalAlignment.Center);
            path.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            path.SetValue(UIElement.OpacityProperty, 0.0);

            border.AppendChild(path);

            FrameworkElementFactory presenter = new(typeof(Controls.ContentPresenter));

            presenter.SetBinding(
                Controls.ContentPresenter.ContentProperty,
                new WPF.Data.Binding(nameof(Content))
                {
                    RelativeSource = new WPF.Data.RelativeSource(WPF.Data.RelativeSourceMode.TemplatedParent)
                });

            // Espacement case/libellé piloté par BoxSpacing (via converter)
            presenter.SetBinding(
                FrameworkElement.MarginProperty,
                new WPF.Data.Binding(nameof(BoxSpacing))
                {
                    RelativeSource = new WPF.Data.RelativeSource(WPF.Data.RelativeSourceMode.TemplatedParent),
                    Converter = new DoubleToLeftMarginConverter()
                });

            presenter.SetValue(
                Controls.ContentPresenter.VerticalAlignmentProperty,
                VerticalAlignment.Center);

            panel.AppendChild(border);
            panel.AppendChild(presenter);

            Controls.ControlTemplate template = new(typeof(CheckBoxCustom));
            template.VisualTree = panel;

            // Coché : affiche le crochet.
            // (Le fond reste celui de Background, déjà implémenté par défaut sur CheckBox.)
            Trigger checkedTrigger = new()
            {
                Property = ToggleButton.IsCheckedProperty,
                Value = true
            };
            checkedTrigger.Setters.Add(new Setter
            {
                Property = UIElement.OpacityProperty,
                Value = 1.0,
                TargetName = "optionMark"
            });
            template.Triggers.Add(checkedTrigger);

            // Survol : bordure BorderHover (binding dynamique, pas de snapshot)
            Trigger mouseTrigger = new()
            {
                Property = UIElement.IsMouseOverProperty,
                Value = true
            };
            mouseTrigger.Setters.Add(new Setter
            {
                Property = Controls.Border.BorderBrushProperty,
                TargetName = "checkBoxBorder",
                Value = new WPF.Data.Binding(nameof(BorderHover))
                {
                    RelativeSource = new WPF.Data.RelativeSource(WPF.Data.RelativeSourceMode.TemplatedParent)
                }
            });
            template.Triggers.Add(mouseTrigger);

            return template;
        }

        #endregion
    }

    #endregion

    #region ToggleSwitchCustom

    /// <summary>
    /// Interrupteur on/off style mobile.
    /// Hérite de CheckBox : IsChecked, Checked/Unchecked, bindings TwoWay
    /// et clavier (Espace) fonctionnent sans aucun code supplémentaire.
    /// </summary>
    public class ToggleSwitchCustom : Controls.CheckBox
    {
        #region Dependency Properties

        /// <summary>
        /// Couleur de la piste quand l'interrupteur est ON.
        /// </summary>
        public static readonly DependencyProperty OnBrushProperty =
            DependencyProperty.Register(
                nameof(OnBrush),
                typeof(Brush),
                typeof(ToggleSwitchCustom),
                new PropertyMetadata(new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#FF00D1FF"))));

        /// <summary>
        /// Couleur de la piste quand l'interrupteur est OFF.
        /// </summary>
        public static readonly DependencyProperty OffBrushProperty =
            DependencyProperty.Register(
                nameof(OffBrush),
                typeof(Brush),
                typeof(ToggleSwitchCustom),
                new PropertyMetadata(new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#FF555555"))));

        /// <summary>
        /// Couleur de la pastille mobile.
        /// </summary>
        public static readonly DependencyProperty ThumbBrushProperty =
            DependencyProperty.Register(
                nameof(ThumbBrush),
                typeof(Brush),
                typeof(ToggleSwitchCustom),
                new PropertyMetadata(Media.Brushes.White));

        public static readonly DependencyProperty NeonProperty =
            DependencyProperty.Register(
                nameof(Neon),
                typeof(bool),
                typeof(ToggleSwitchCustom),
                new PropertyMetadata(false, OnNeonPropertyChanged));

        public static readonly DependencyProperty NeonRadiusProperty =
            DependencyProperty.Register(
                nameof(NeonRadius),
                typeof(double),
                typeof(ToggleSwitchCustom),
                new PropertyMetadata(20.0, OnNeonPropertyChanged));

        public static readonly DependencyProperty NeonColorProperty =
            DependencyProperty.Register(
                nameof(NeonColor),
                typeof(Media.Color),
                typeof(ToggleSwitchCustom),
                new PropertyMetadata(Media.Colors.Cyan, OnNeonPropertyChanged));

        #endregion

        #region Propriétés

        public Brush OnBrush
        {
            get => (Brush)GetValue(OnBrushProperty);
            set => SetValue(OnBrushProperty, value);
        }

        public Brush OffBrush
        {
            get => (Brush)GetValue(OffBrushProperty);
            set => SetValue(OffBrushProperty, value);
        }

        public Brush ThumbBrush
        {
            get => (Brush)GetValue(ThumbBrushProperty);
            set => SetValue(ThumbBrushProperty, value);
        }

        public bool Neon
        {
            get => (bool)GetValue(NeonProperty);
            set => SetValue(NeonProperty, value);
        }

        public Media.Color NeonColor
        {
            get => (Media.Color)GetValue(NeonColorProperty);
            set => SetValue(NeonColorProperty, value);
        }

        public double NeonRadius
        {
            get => (double)GetValue(NeonRadiusProperty);
            set => SetValue(NeonRadiusProperty, value);
        }

        #endregion

        #region Constructeur

        public ToggleSwitchCustom()
        {
            Template = BuildTemplate();
            RefreshNeon();
        }

        #endregion

        #region Callbacks

        private static void OnNeonPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ToggleSwitchCustom toggle)
            {
                toggle.RefreshNeon();
            }
        }

        #endregion

        #region Néon (lueur sur la piste, libellé net)

        // La lueur est appliquée sur la piste ("switchTrack"), pas sur le contrôle
        // entier : le libellé à côté reste net et lisible.
        private Border _glowTrack;
        private Border _thumb;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _glowTrack = Template.FindName("switchTrack", this) as Border;
            _thumb = Template.FindName("switchThumb", this) as Border;
            RefreshNeon();

            // FIX (2026-11-05) : position initiale de la pastille.
            // Les EnterActions/ExitActions du trigger ne se déclenchent que sur
            // une TRANSITION de IsChecked, pas au chargement quand IsChecked
            // vaut déjà True : les setters du trigger, eux, s'appliquent dès
            // le départ (piste cyan), mais l'animation de glisse ne part pas.
            // Résultat sans ce fix : dans la Vitrine, les 3 toggles avec
            // IsChecked="True" affichent une piste cyan avec la pastille
            // restée à gauche — incohérent, et ça ne se répare qu'au premier
            // clic (décoche puis recoche). On place donc la pastille ici,
            // sans animation (c'est l'état initial, pas un mouvement) ; les
            // clics suivants passent toujours par les animations du trigger.
            if (_thumb != null)
            {
                _thumb.Margin = IsChecked == true
                    ? new Thickness(23, 0, 0, 0)
                    : new Thickness(3, 0, 0, 0);
            }
        }

        private void RefreshNeon()
        {
            if (_glowTrack != null)
            {
                NeonHelper.ApplyEffect(_glowTrack, Neon, NeonColor, NeonRadius);
                Effect = null;
            }
            else
            {
                NeonHelper.ApplyEffect(this, Neon, NeonColor, NeonRadius);
            }
        }

        #endregion

        #region Template

        private Controls.ControlTemplate BuildTemplate()
        {
            FrameworkElementFactory panel = new(typeof(Controls.StackPanel));
            panel.SetValue(Controls.StackPanel.OrientationProperty, Controls.Orientation.Horizontal);
            panel.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);

            // Piste (46x26, forme pilule)
            FrameworkElementFactory track = new(typeof(Controls.Border));
            track.Name = "switchTrack";
            track.SetValue(Controls.Border.WidthProperty, 46.0);
            track.SetValue(Controls.Border.HeightProperty, 26.0);
            track.SetValue(Controls.Border.CornerRadiusProperty, new CornerRadius(13));
            track.SetBinding(
                Controls.Border.BackgroundProperty,
                new WPF.Data.Binding(nameof(OffBrush))
                {
                    RelativeSource = new WPF.Data.RelativeSource(WPF.Data.RelativeSourceMode.TemplatedParent)
                });

            // Pastille mobile (20px, centrée verticalement, à gauche par défaut)
            FrameworkElementFactory thumb = new(typeof(Controls.Border));
            thumb.Name = "switchThumb";
            thumb.SetValue(Controls.Border.WidthProperty, 20.0);
            thumb.SetValue(Controls.Border.HeightProperty, 20.0);
            thumb.SetValue(Controls.Border.CornerRadiusProperty, new CornerRadius(10));
            thumb.SetBinding(
                Controls.Border.BackgroundProperty,
                new WPF.Data.Binding(nameof(ThumbBrush))
                {
                    RelativeSource = new WPF.Data.RelativeSource(WPF.Data.RelativeSourceMode.TemplatedParent)
                });
            thumb.SetValue(FrameworkElement.HorizontalAlignmentProperty, WPF.HorizontalAlignment.Left);
            thumb.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            thumb.SetValue(FrameworkElement.MarginProperty, new Thickness(3, 0, 0, 0));

            track.AppendChild(thumb);

            // Libellé optionnel à droite
            FrameworkElementFactory presenter = new(typeof(Controls.ContentPresenter));
            presenter.SetBinding(
                Controls.ContentPresenter.ContentProperty,
                new WPF.Data.Binding(nameof(Content))
                {
                    RelativeSource = new WPF.Data.RelativeSource(WPF.Data.RelativeSourceMode.TemplatedParent)
                });
            presenter.SetValue(FrameworkElement.MarginProperty, new Thickness(8, 0, 0, 0));
            presenter.SetValue(Controls.ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

            panel.AppendChild(track);
            panel.AppendChild(presenter);

            Controls.ControlTemplate template = new(typeof(ToggleSwitchCustom));
            template.VisualTree = panel;

            // ON : piste colorée + la pastille GLISSE vers la droite
            // (mini animation de 150 ms au lieu de se téléporter).
            // Les triggers se désactivent seuls quand IsChecked=false -> retour à l'état OFF.
            Trigger onTrigger = new()
            {
                Property = ToggleButton.IsCheckedProperty,
                Value = true
            };
            onTrigger.Setters.Add(new Setter
            {
                Property = Controls.Border.BackgroundProperty,
                TargetName = "switchTrack",
                Value = new WPF.Data.Binding(nameof(OnBrush))
                {
                    RelativeSource = new WPF.Data.RelativeSource(WPF.Data.RelativeSourceMode.TemplatedParent)
                }
            });

            // Glisse vers la droite : marge gauche 3 -> 23 (46 piste - 20 pastille - 3 marge).
            // Sans "From" : l'animation part de la valeur actuelle, donc ça marche
            // dans tous les cas (clic, binding, état initial).
            Storyboard slideOn = new();
            ThicknessAnimation animOn = new()
            {
                To = new Thickness(23, 0, 0, 0),
                Duration = new Duration(TimeSpan.FromMilliseconds(150))
            };
            Storyboard.SetTargetName(animOn, "switchThumb");
            Storyboard.SetTargetProperty(animOn, new PropertyPath(FrameworkElement.MarginProperty));
            slideOn.Children.Add(animOn);
            onTrigger.EnterActions.Add(new BeginStoryboard { Storyboard = slideOn });

            // Retour vers la gauche quand on décoche
            Storyboard slideOff = new();
            ThicknessAnimation animOff = new()
            {
                To = new Thickness(3, 0, 0, 0),
                Duration = new Duration(TimeSpan.FromMilliseconds(150))
            };
            Storyboard.SetTargetName(animOff, "switchThumb");
            Storyboard.SetTargetProperty(animOff, new PropertyPath(FrameworkElement.MarginProperty));
            slideOff.Children.Add(animOff);
            onTrigger.ExitActions.Add(new BeginStoryboard { Storyboard = slideOff });

            template.Triggers.Add(onTrigger);

            return template;
        }

        #endregion
    }

    #endregion

    #region CardCustom

    /// <summary>
    /// Panneau "carte" : conteneur arrondi avec titre optionnel.
    /// Utile pour structurer des sections (ex: panneau d'options).
    /// </summary>
    public class CardCustom : ContentControl
    {
        #region Dependency Properties

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(CardCustom),
                new PropertyMetadata(new CornerRadius(10)));

        /// <summary>
        /// Titre affiché en haut de la carte. Vide = pas de titre.
        /// </summary>
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(CardCustom),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty TitleForegroundProperty =
            DependencyProperty.Register(
                nameof(TitleForeground),
                typeof(Brush),
                typeof(CardCustom),
                new PropertyMetadata(Media.Brushes.White));

        public static readonly DependencyProperty TitleFontSizeProperty =
            DependencyProperty.Register(
                nameof(TitleFontSize),
                typeof(double),
                typeof(CardCustom),
                new PropertyMetadata(14.0));

        #endregion

        #region Propriétés

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public Brush TitleForeground
        {
            get => (Brush)GetValue(TitleForegroundProperty);
            set => SetValue(TitleForegroundProperty, value);
        }

        public double TitleFontSize
        {
            get => (double)GetValue(TitleFontSizeProperty);
            set => SetValue(TitleFontSizeProperty, value);
        }

        #endregion

        #region Constructeur

        public CardCustom()
        {
            Template = BuildTemplate();
        }

        #endregion

        #region Template

        private Controls.ControlTemplate BuildTemplate()
        {
            FrameworkElementFactory border = new(typeof(Controls.Border));

            border.SetBinding(
                Controls.Border.BackgroundProperty,
                new WPF.Data.Binding(nameof(Background))
                {
                    RelativeSource = new WPF.Data.RelativeSource(WPF.Data.RelativeSourceMode.TemplatedParent)
                });
            border.SetBinding(
                Controls.Border.BorderBrushProperty,
                new WPF.Data.Binding(nameof(BorderBrush))
                {
                    RelativeSource = new WPF.Data.RelativeSource(WPF.Data.RelativeSourceMode.TemplatedParent)
                });
            border.SetBinding(
                Controls.Border.BorderThicknessProperty,
                new WPF.Data.Binding(nameof(BorderThickness))
                {
                    RelativeSource = new WPF.Data.RelativeSource(WPF.Data.RelativeSourceMode.TemplatedParent)
                });
            border.SetBinding(
                Controls.Border.CornerRadiusProperty,
                new WPF.Data.Binding(nameof(CornerRadius))
                {
                    RelativeSource = new WPF.Data.RelativeSource(WPF.Data.RelativeSourceMode.TemplatedParent)
                });
            border.SetValue(Controls.Border.PaddingProperty, new Thickness(12));

            FrameworkElementFactory panel = new(typeof(Controls.StackPanel));

            FrameworkElementFactory title = new(typeof(Controls.TextBlock));
            title.Name = "cardTitle";
            title.SetBinding(
                Controls.TextBlock.TextProperty,
                new WPF.Data.Binding(nameof(Title))
                {
                    RelativeSource = new WPF.Data.RelativeSource(WPF.Data.RelativeSourceMode.TemplatedParent)
                });
            title.SetBinding(
                Controls.TextBlock.ForegroundProperty,
                new WPF.Data.Binding(nameof(TitleForeground))
                {
                    RelativeSource = new WPF.Data.RelativeSource(WPF.Data.RelativeSourceMode.TemplatedParent)
                });
            title.SetBinding(
                Controls.TextBlock.FontSizeProperty,
                new WPF.Data.Binding(nameof(TitleFontSize))
                {
                    RelativeSource = new WPF.Data.RelativeSource(WPF.Data.RelativeSourceMode.TemplatedParent)
                });
            title.SetValue(Controls.TextBlock.FontWeightProperty, FontWeights.SemiBold);
            title.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 0, 0, 8));

            FrameworkElementFactory presenter = new(typeof(Controls.ContentPresenter));
            presenter.SetBinding(
                Controls.ContentPresenter.ContentProperty,
                new WPF.Data.Binding(nameof(Content))
                {
                    RelativeSource = new WPF.Data.RelativeSource(WPF.Data.RelativeSourceMode.TemplatedParent)
                });

            panel.AppendChild(title);
            panel.AppendChild(presenter);
            border.AppendChild(panel);

            Controls.ControlTemplate template = new(typeof(CardCustom));
            template.VisualTree = border;

            // Pas de titre -> on cache la zone de titre
            Trigger noTitleTrigger = new()
            {
                Property = TitleProperty,
                Value = string.Empty
            };
            noTitleTrigger.Setters.Add(new Setter
            {
                Property = UIElement.VisibilityProperty,
                Value = Visibility.Collapsed,
                TargetName = "cardTitle"
            });
            template.Triggers.Add(noTitleTrigger);

            return template;
        }

        #endregion
    }

    #endregion

    #region SliderCustom

    /// <summary>
    /// Slider custom : glisser de la pastille, clic sur la piste pour sauter,
    /// flèches du clavier. Value/Minimum/Maximum/Ticks hérités de Slider.
    /// Le template est reconstruit si Orientation change.
    /// </summary>
    public class SliderCustom : Slider
    {
        #region Dependency Properties

        /// <summary>
        /// Couleur de la piste.
        /// </summary>
        public static readonly DependencyProperty TrackBrushProperty =
            DependencyProperty.Register(
                nameof(TrackBrush),
                typeof(Brush),
                typeof(SliderCustom),
                new PropertyMetadata(new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#FF555555"))));

        /// <summary>
        /// Couleur de la pastille.
        /// </summary>
        public static readonly DependencyProperty ThumbBrushProperty =
            DependencyProperty.Register(
                nameof(ThumbBrush),
                typeof(Brush),
                typeof(SliderCustom),
                new PropertyMetadata(new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#FF00D1FF"))));

        /// <summary>
        /// Diamètre (en px) de la pastille.
        /// </summary>
        public static readonly DependencyProperty ThumbSizeProperty =
            DependencyProperty.Register(
                nameof(ThumbSize),
                typeof(double),
                typeof(SliderCustom),
                new PropertyMetadata(18.0));

        /// <summary>
        /// Épaisseur (en px) de la piste.
        /// </summary>
        public static readonly DependencyProperty TrackHeightProperty =
            DependencyProperty.Register(
                nameof(TrackHeight),
                typeof(double),
                typeof(SliderCustom),
                new PropertyMetadata(6.0));

        /// <summary>
        /// Forme de la piste : 0 = rectangle aux coins vifs,
        /// petite valeur = coins légèrement arrondis,
        /// grande valeur = pilule (défaut).
        /// </summary>
        public static readonly DependencyProperty TrackCornerRadiusProperty =
            DependencyProperty.Register(
                nameof(TrackCornerRadius),
                typeof(CornerRadius),
                typeof(SliderCustom),
                new PropertyMetadata(new CornerRadius(1000)));

        /// <summary>
        /// Forme de la pastille : 0 = carré, grande valeur = rond (défaut).
        /// </summary>
        public static readonly DependencyProperty ThumbCornerRadiusProperty =
            DependencyProperty.Register(
                nameof(ThumbCornerRadius),
                typeof(CornerRadius),
                typeof(SliderCustom),
                new PropertyMetadata(new CornerRadius(1000)));

        public static readonly DependencyProperty NeonProperty =
            DependencyProperty.Register(
                nameof(Neon),
                typeof(bool),
                typeof(SliderCustom),
                new PropertyMetadata(false, OnNeonPropertyChanged));

        public static readonly DependencyProperty NeonRadiusProperty =
            DependencyProperty.Register(
                nameof(NeonRadius),
                typeof(double),
                typeof(SliderCustom),
                new PropertyMetadata(20.0, OnNeonPropertyChanged));

        public static readonly DependencyProperty NeonColorProperty =
            DependencyProperty.Register(
                nameof(NeonColor),
                typeof(Media.Color),
                typeof(SliderCustom),
                new PropertyMetadata(Media.Colors.Cyan, OnNeonPropertyChanged));

        #endregion

        #region Propriétés

        public Brush TrackBrush
        {
            get => (Brush)GetValue(TrackBrushProperty);
            set => SetValue(TrackBrushProperty, value);
        }

        public Brush ThumbBrush
        {
            get => (Brush)GetValue(ThumbBrushProperty);
            set => SetValue(ThumbBrushProperty, value);
        }

        public double ThumbSize
        {
            get => (double)GetValue(ThumbSizeProperty);
            set => SetValue(ThumbSizeProperty, value);
        }

        public double TrackHeight
        {
            get => (double)GetValue(TrackHeightProperty);
            set => SetValue(TrackHeightProperty, value);
        }

        public CornerRadius TrackCornerRadius
        {
            get => (CornerRadius)GetValue(TrackCornerRadiusProperty);
            set => SetValue(TrackCornerRadiusProperty, value);
        }

        public CornerRadius ThumbCornerRadius
        {
            get => (CornerRadius)GetValue(ThumbCornerRadiusProperty);
            set => SetValue(ThumbCornerRadiusProperty, value);
        }

        public bool Neon
        {
            get => (bool)GetValue(NeonProperty);
            set => SetValue(NeonProperty, value);
        }

        public Media.Color NeonColor
        {
            get => (Media.Color)GetValue(NeonColorProperty);
            set => SetValue(NeonColorProperty, value);
        }

        public double NeonRadius
        {
            get => (double)GetValue(NeonRadiusProperty);
            set => SetValue(NeonRadiusProperty, value);
        }

        #endregion

        #region Champs

        private Border _track;
        private Thumb _thumb;
        private Grid _grid;

        #endregion

        #region Constructeur

        public SliderCustom()
        {
            Template = BuildTemplate();
            NeonHelper.ApplyEffect(this, Neon, NeonColor, NeonRadius);
        }

        #endregion

        #region Callbacks

        private static void OnNeonPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SliderCustom slider)
            {
                NeonHelper.ApplyEffect(slider, slider.Neon, slider.NeonColor, slider.NeonRadius);
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            // Le layout dépend de l'orientation : on reconstruit le template.
            // (Changement rare, aucun impact en utilisation normale.)
            if (e.Property == OrientationProperty)
            {
                Template = BuildTemplate();
            }

            // ThumbSize influence la course de la pastille : on la repositionne
            // (sinon elle reste décalée jusqu'au prochain changement de valeur).
            if (e.Property == ThumbSizeProperty)
            {
                PositionThumb();
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // On se désabonne d'abord (sécurité si le template est réappliqué)
            if (_thumb != null) _thumb.DragDelta -= Thumb_DragDelta;
            if (_grid != null) _grid.MouseLeftButtonDown -= Track_MouseDown;

            _track = Template.FindName("trackBorder", this) as Border;
            _thumb = Template.FindName("sliderThumb", this) as Thumb;
            // (Repli : si FindName ne retrouvait pas la grille racine,
            // on la prend comme premier enfant visuel — c'est elle.)
            _grid = Template.FindName("sliderGrid", this) as Grid
                ?? (VisualTreeHelper.GetChildrenCount(this) > 0
                    ? VisualTreeHelper.GetChild(this, 0) as Grid
                    : null);

            if (_thumb != null) _thumb.DragDelta += Thumb_DragDelta;
            // FIX (2026-11-05) : le clic est écouté sur la grille entière,
            // pas juste sur la piste de 6 px (voir Track_MouseDown).
            if (_grid != null) _grid.MouseLeftButtonDown += Track_MouseDown;

            PositionThumb();
        }

        protected override void OnValueChanged(double oldValue, double newValue)
        {
            base.OnValueChanged(oldValue, newValue);
            PositionThumb();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            PositionThumb();
        }

        #endregion

        #region Positionnement

        private double TrackLength()
        {
            if (_track == null) return 0;
            return Orientation == Orientation.Horizontal ? _track.ActualWidth : _track.ActualHeight;
        }

        /// <summary>
        /// Place la pastille selon Value. Appelé à chaque changement de
        /// valeur, de taille ou de template.
        /// </summary>
        private void PositionThumb()
        {
            if (_track == null || _thumb == null) return;

            double range = Maximum - Minimum;
            double ratio = range <= 0 ? 0 : (Value - Minimum) / range;
            ratio = Math.Max(0, Math.Min(1, ratio)); // clamp 0..1

            // La pastille voyage sur (piste - sa propre taille)
            double pos = ratio * Math.Max(0, TrackLength() - ThumbSize);

            if (Orientation == Orientation.Horizontal)
            {
                _thumb.Margin = new Thickness(pos, 0, 0, 0);
            }
            else
            {
                // Vertical : 0 en bas. Avec VerticalAlignment=Bottom,
                // une marge basse = pos place la pastille en haut quand Value = max.
                _thumb.Margin = new Thickness(0, 0, 0, pos);
            }
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double travel = Math.Max(0, TrackLength() - ThumbSize);
            if (travel <= 0) return;

            double range = Maximum - Minimum;
            double newValue;

            if (Orientation == Orientation.Horizontal)
            {
                newValue = Value + e.HorizontalChange / travel * range;
            }
            else
            {
                // Vers le haut (VerticalChange négatif) = augmenter
                newValue = Value - e.VerticalChange / travel * range;
            }

            Value = ApplySnap(newValue);
        }

        private void Track_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_track == null) return;

            // FIX (2026-11-05) : le handler est sur la grille entière, donc
            // un clic sur la pastille arrive aussi ici — on l'ignore, sinon
            // la valeur sauterait avant même que le drag commence.
            if (_thumb != null && _thumb.IsMouseOver) return;

            // Le clic donne le focus au slider : les flèches ← → marchent
            // juste après, sans avoir à faire Tab d'abord.
            Focus();

            double travel = Math.Max(0, TrackLength() - ThumbSize);
            if (travel <= 0) return;

            Point p = e.GetPosition(_track);
            double ratio;

            if (Orientation == Orientation.Horizontal)
            {
                ratio = (p.X - ThumbSize / 2) / travel;
            }
            else
            {
                ratio = 1 - (p.Y - ThumbSize / 2) / travel;
            }

            ratio = Math.Max(0, Math.Min(1, ratio));
            Value = ApplySnap(Minimum + ratio * (Maximum - Minimum));
        }

        /// <summary>
        /// Applique le snap sur les ticks quand IsSnapToTickEnabled.
        /// Fait explicitement ici car la valeur est posée par code (drag, clic,
        /// clavier) et on ne peut pas compter sur la coercition de la base.
        /// Idempotent : snapper une valeur déjà snappée ne change rien, donc
        /// aucun risque si la base le fait aussi.
        /// </summary>
        private double ApplySnap(double value)
        {
            if (!IsSnapToTickEnabled) return value;

            double freq = TickFrequency;
            if (freq <= 0) return value;

            double snapped = Math.Round((value - Minimum) / freq, MidpointRounding.AwayFromZero) * freq + Minimum;
            return Math.Max(Minimum, Math.Min(Maximum, snapped));
        }

        #endregion

        #region Template

        private Controls.ControlTemplate BuildTemplate()
        {
            bool vertical = Orientation == Orientation.Vertical;

            // Binding "remonte d'un niveau" : utilisé pour les propriétés
            // de la pastille, dont le TemplatedParent est le Thumb lui-même.
            RelativeSource ancestor =
                new(RelativeSourceMode.FindAncestor, typeof(SliderCustom), 1);
            RelativeSource templatedParent =
                new(RelativeSourceMode.TemplatedParent);

            FrameworkElementFactory grid = new(typeof(Grid));
            grid.Name = "sliderGrid";
            grid.SetValue(Grid.BackgroundProperty, Brushes.Transparent); // zone cliquable

            if (vertical)
            {
                grid.SetBinding(
                    FrameworkElement.MinWidthProperty,
                    new WPF.Data.Binding(nameof(ThumbSize)) { RelativeSource = templatedParent });
            }
            else
            {
                grid.SetBinding(
                    FrameworkElement.MinHeightProperty,
                    new WPF.Data.Binding(nameof(ThumbSize)) { RelativeSource = templatedParent });
            }

            // Piste
            FrameworkElementFactory track = new(typeof(Border));
            track.Name = "trackBorder";
            // Forme pilotée par TrackCornerRadius (binding dynamique)
            track.SetBinding(
                Border.CornerRadiusProperty,
                new WPF.Data.Binding(nameof(TrackCornerRadius)) { RelativeSource = templatedParent });
            track.SetBinding(
                Border.BackgroundProperty,
                new WPF.Data.Binding(nameof(TrackBrush)) { RelativeSource = templatedParent });

            if (vertical)
            {
                track.SetBinding(
                    Border.WidthProperty,
                    new WPF.Data.Binding(nameof(TrackHeight)) { RelativeSource = templatedParent });
                track.SetValue(FrameworkElement.HorizontalAlignmentProperty, WPF.HorizontalAlignment.Center);
                track.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Stretch);
            }
            else
            {
                track.SetBinding(
                    Border.HeightProperty,
                    new WPF.Data.Binding(nameof(TrackHeight)) { RelativeSource = templatedParent });
                track.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
                track.SetValue(FrameworkElement.HorizontalAlignmentProperty, WPF.HorizontalAlignment.Stretch);
            }

            // Pastille : template avec bindings vers le SliderCustom
            Controls.ControlTemplate thumbTemplate = new(typeof(Thumb));
            FrameworkElementFactory thumbBorder = new(typeof(Border));
            // Forme pilotée par ThumbCornerRadius (binding dynamique)
            thumbBorder.SetBinding(
                Border.CornerRadiusProperty,
                new WPF.Data.Binding(nameof(ThumbCornerRadius)) { RelativeSource = ancestor });
            thumbBorder.SetBinding(
                Border.BackgroundProperty,
                new WPF.Data.Binding(nameof(ThumbBrush)) { RelativeSource = ancestor });
            thumbBorder.SetBinding(
                Border.WidthProperty,
                new WPF.Data.Binding(nameof(ThumbSize)) { RelativeSource = ancestor });
            thumbBorder.SetBinding(
                Border.HeightProperty,
                new WPF.Data.Binding(nameof(ThumbSize)) { RelativeSource = ancestor });
            thumbTemplate.VisualTree = thumbBorder;

            FrameworkElementFactory thumb = new(typeof(Thumb));
            thumb.Name = "sliderThumb";
            thumb.SetValue(Thumb.TemplateProperty, thumbTemplate);
            thumb.SetValue(FrameworkElement.FocusableProperty, false); // le focus reste sur le slider (clavier)

            if (vertical)
            {
                thumb.SetValue(FrameworkElement.HorizontalAlignmentProperty, WPF.HorizontalAlignment.Center);
                thumb.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Bottom);
            }
            else
            {
                thumb.SetValue(FrameworkElement.HorizontalAlignmentProperty, WPF.HorizontalAlignment.Left);
                thumb.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            }

            grid.AppendChild(track);
            grid.AppendChild(thumb);

            Controls.ControlTemplate template = new(typeof(SliderCustom));
            template.VisualTree = grid;

            return template;
        }

        #endregion
    }

    #endregion

    #region ListBoxCustom

    /// <summary>
    /// ListBox avec coins arrondis et couleurs de l'item sélectionné
    /// pilotables par propriétés (au lieu du bleu système de WPF).
    /// Hérite de ListBox : ItemsSource, SelectedItem, SelectionChanged,
    /// clavier (flèches) et scroll fonctionnent sans aucun code
    /// supplémentaire. Exemple d'usage dans la Vitrine / une Page :
    /// ItemsSource = liste, SelectedItem = l'objet choisi directement.
    /// </summary>
    public class ListBoxCustom : Controls.ListBox
    {
        #region Dependency Properties

        /// <summary>
        /// Coins arrondis de la liste elle-même. 0 = rectangle vif.
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(ListBoxCustom),
                new PropertyMetadata(new CornerRadius(10)));

        /// <summary>
        /// Coins arrondis de chaque item (la carte de fond de la ligne).
        /// 0 = rectangle vif.
        /// </summary>
        public static readonly DependencyProperty ItemCornerRadiusProperty =
            DependencyProperty.Register(
                nameof(ItemCornerRadius),
                typeof(CornerRadius),
                typeof(ListBoxCustom),
                new PropertyMetadata(new CornerRadius(8)));

        /// <summary>
        /// Fond de l'item sélectionné.
        /// </summary>
        public static readonly DependencyProperty SelectedItemBackgroundProperty =
            DependencyProperty.Register(
                nameof(SelectedItemBackground),
                typeof(Brush),
                typeof(ListBoxCustom),
                new PropertyMetadata(new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#FF00D1FF"))));

        /// <summary>
        /// Couleur du texte de l'item sélectionné.
        /// </summary>
        public static readonly DependencyProperty SelectedItemForegroundProperty =
            DependencyProperty.Register(
                nameof(SelectedItemForeground),
                typeof(Brush),
                typeof(ListBoxCustom),
                new PropertyMetadata(Media.Brushes.Black));

        /// <summary>
        /// Couleur de la bordure de l'item sélectionné.
        /// </summary>
        public static readonly DependencyProperty SelectedItemBorderBrushProperty =
            DependencyProperty.Register(
                nameof(SelectedItemBorderBrush),
                typeof(Brush),
                typeof(ListBoxCustom),
                new PropertyMetadata(new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#FF00D1FF"))));

        #endregion

        #region Propriétés

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public CornerRadius ItemCornerRadius
        {
            get => (CornerRadius)GetValue(ItemCornerRadiusProperty);
            set => SetValue(ItemCornerRadiusProperty, value);
        }

        public Brush SelectedItemBackground
        {
            get => (Brush)GetValue(SelectedItemBackgroundProperty);
            set => SetValue(SelectedItemBackgroundProperty, value);
        }

        public Brush SelectedItemForeground
        {
            get => (Brush)GetValue(SelectedItemForegroundProperty);
            set => SetValue(SelectedItemForegroundProperty, value);
        }

        public Brush SelectedItemBorderBrush
        {
            get => (Brush)GetValue(SelectedItemBorderBrushProperty);
            set => SetValue(SelectedItemBorderBrushProperty, value);
        }

        #endregion

        #region Constructeur

        public ListBoxCustom()
        {
            Template = BuildTemplate();
            // Style des lignes : si tu définis ton propre ItemContainerStyle
            // en XAML, le tien remplace celui-ci (posé après le constructeur).
            ItemContainerStyle = BuildItemContainerStyle();
        }

        #endregion

        #region Helpers

        // Binding vers une propriété du ListBoxCustom qui contient la ligne.
        // (Dans le template d'une ligne, TemplatedParent = la ligne elle-même,
        // pas la liste : il faut remonter d'un cran avec FindAncestor.)
        private static WPF.Data.Binding AncestorBinding(string path)
        {
            return new WPF.Data.Binding(path)
            {
                RelativeSource = new WPF.Data.RelativeSource(
                    WPF.Data.RelativeSourceMode.FindAncestor, typeof(ListBoxCustom), 1)
            };
        }

        #endregion

        #region Template de la liste

        private static Controls.ControlTemplate BuildTemplate()
        {
            WPF.Data.RelativeSource templatedParent =
                new(WPF.Data.RelativeSourceMode.TemplatedParent);

            // Bordure externe : c'est elle qui porte le CornerRadius de la
            // liste, plus Background / BorderBrush / BorderThickness.
            // Le Padding de 4 garde les items à l'intérieur des coins arrondis.
            FrameworkElementFactory border = new(typeof(Controls.Border));
            border.Name = "listBorder";
            border.SetBinding(
                Controls.Border.BackgroundProperty,
                new WPF.Data.Binding(nameof(Background)) { RelativeSource = templatedParent });
            border.SetBinding(
                Controls.Border.BorderBrushProperty,
                new WPF.Data.Binding(nameof(BorderBrush)) { RelativeSource = templatedParent });
            border.SetBinding(
                Controls.Border.BorderThicknessProperty,
                new WPF.Data.Binding(nameof(BorderThickness)) { RelativeSource = templatedParent });
            border.SetBinding(
                Controls.Border.CornerRadiusProperty,
                new WPF.Data.Binding(nameof(CornerRadius)) { RelativeSource = templatedParent });
            border.SetValue(Controls.Border.PaddingProperty, new Thickness(4));

            FrameworkElementFactory scroll = new(typeof(Controls.ScrollViewer));
            scroll.SetValue(Controls.ScrollViewer.VerticalScrollBarVisibilityProperty,
                Controls.ScrollBarVisibility.Auto);
            scroll.SetValue(Controls.ScrollViewer.HorizontalScrollBarVisibilityProperty,
                Controls.ScrollBarVisibility.Disabled);
            scroll.SetValue(Controls.ScrollViewer.CanContentScrollProperty, true);
            scroll.SetValue(FrameworkElement.FocusableProperty, false);

            FrameworkElementFactory presenter = new(typeof(Controls.ItemsPresenter));

            scroll.AppendChild(presenter);
            border.AppendChild(scroll);

            Controls.ControlTemplate template = new(typeof(ListBoxCustom));
            template.VisualTree = border;

            return template;
        }

        #endregion

        #region Style des lignes (items)

        private static Style BuildItemContainerStyle()
        {
            WPF.Data.RelativeSource templatedParent =
                new(WPF.Data.RelativeSourceMode.TemplatedParent);

            Style style = new(typeof(Controls.ListBoxItem));
            style.Setters.Add(new Setter(Controls.ListBoxItem.BackgroundProperty,
                Media.Brushes.Transparent));
            style.Setters.Add(new Setter(Controls.ListBoxItem.BorderThicknessProperty,
                new Thickness(1)));
            style.Setters.Add(new Setter(Controls.ListBoxItem.PaddingProperty,
                new Thickness(8, 6, 8, 6)));
            style.Setters.Add(new Setter(Controls.ListBoxItem.MarginProperty,
                new Thickness(0, 0, 0, 4)));
            style.Setters.Add(new Setter(Controls.ListBoxItem.HorizontalContentAlignmentProperty,
                WPF.HorizontalAlignment.Stretch));

            // Template d'une ligne : un Border (le fond de la ligne) +
            // un ContentPresenter. ContentTemplate est bindé aussi, sinon
            // un ItemTemplate / DataTemplate défini sur la liste serait ignoré.
            Controls.ControlTemplate itemTemplate = new(typeof(Controls.ListBoxItem));

            FrameworkElementFactory itemBorder = new(typeof(Controls.Border));
            itemBorder.Name = "itemBorder";
            itemBorder.SetBinding(
                Controls.Border.BackgroundProperty,
                new WPF.Data.Binding(nameof(Background)) { RelativeSource = templatedParent });
            itemBorder.SetBinding(
                Controls.Border.BorderBrushProperty,
                new WPF.Data.Binding(nameof(BorderBrush)) { RelativeSource = templatedParent });
            itemBorder.SetBinding(
                Controls.Border.BorderThicknessProperty,
                new WPF.Data.Binding(nameof(BorderThickness)) { RelativeSource = templatedParent });
            itemBorder.SetBinding(
                Controls.Border.CornerRadiusProperty,
                AncestorBinding(nameof(ItemCornerRadius)));
            itemBorder.SetBinding(
                Controls.Border.PaddingProperty,
                new WPF.Data.Binding(nameof(Padding)) { RelativeSource = templatedParent });

            FrameworkElementFactory presenter = new(typeof(Controls.ContentPresenter));
            presenter.SetBinding(
                Controls.ContentPresenter.ContentProperty,
                new WPF.Data.Binding("Content") { RelativeSource = templatedParent });
            presenter.SetBinding(
                Controls.ContentPresenter.ContentTemplateProperty,
                new WPF.Data.Binding("ContentTemplate") { RelativeSource = templatedParent });
            presenter.SetBinding(
                Controls.ContentPresenter.ContentTemplateSelectorProperty,
                new WPF.Data.Binding("ContentTemplateSelector") { RelativeSource = templatedParent });
            presenter.SetBinding(
                Controls.ContentPresenter.HorizontalAlignmentProperty,
                new WPF.Data.Binding(nameof(HorizontalContentAlignment)) { RelativeSource = templatedParent });
            presenter.SetValue(
                Controls.ContentPresenter.VerticalAlignmentProperty,
                VerticalAlignment.Center);

            itemBorder.AppendChild(presenter);
            itemTemplate.VisualTree = itemBorder;

            // Sélectionné : fond + bordure + texte pilotés par les 3
            // propriétés du ListBoxCustom. Sans TargetName, le setter de
            // Foreground s'applique à la ligne elle-même (ListBoxItem) et
            // le texte hérite de sa couleur, sauf s'il définit la sienne.
            Trigger selectedTrigger = new()
            {
                Property = Controls.ListBoxItem.IsSelectedProperty,
                Value = true
            };
            selectedTrigger.Setters.Add(new Setter
            {
                Property = Controls.Border.BackgroundProperty,
                TargetName = "itemBorder",
                Value = AncestorBinding(nameof(SelectedItemBackground))
            });
            selectedTrigger.Setters.Add(new Setter
            {
                Property = Controls.Border.BorderBrushProperty,
                TargetName = "itemBorder",
                Value = AncestorBinding(nameof(SelectedItemBorderBrush))
            });
            selectedTrigger.Setters.Add(new Setter
            {
                Property = Controls.Control.ForegroundProperty,
                Value = AncestorBinding(nameof(SelectedItemForeground))
            });
            itemTemplate.Triggers.Add(selectedTrigger);

            style.Setters.Add(new Setter(Controls.Control.TemplateProperty, itemTemplate));

            return style;
        }

        #endregion
    }

    #endregion

}
