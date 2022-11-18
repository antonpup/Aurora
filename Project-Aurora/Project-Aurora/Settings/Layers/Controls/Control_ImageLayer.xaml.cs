using System.IO;
using System.Windows;
using System.Windows.Forms;
using UserControl = System.Windows.Controls.UserControl;

namespace Aurora.Settings.Layers {

    public partial class Control_ImageLayer : UserControl {
        public Control_ImageLayer() {
            InitializeComponent();
        }

        public Control_ImageLayer(ImageLayerHandler context) {
            InitializeComponent();
            DataContext = context;
        }

        private void SelectImage_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog dialog = new OpenFileDialog {
                Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.png, *.gif, *.bmp, *.tiff, *.tif) | *.jpg; *.jpeg; *.jpe; *.png; *.gif; *.bmp; *.tiff; *.tif",
                Title = "Please select an image."
            };
            if (dialog.ShowDialog() == DialogResult.OK && File.Exists(dialog.FileName))
                (this.DataContext as ImageLayerHandler).Properties._ImagePath = dialog.FileName;
        }
    }
}
