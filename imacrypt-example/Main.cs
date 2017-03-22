using Imacrypt;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using imacrypt_example.Properties;

namespace imacrypt_example
{
    public partial class Main : Form
    {
        private readonly Stopwatch _watch = new Stopwatch();
        public Main()
        {
            InitializeComponent();
        }

        private void SelectBtn_Click(object sender, EventArgs e)
        {
            if (SelectDlg.ShowDialog() == DialogResult.OK)
                FilePathTxt.Text = SelectDlg.FileName;
        }

        public void ChangeStates(bool enabled)
        {
            SelectBtn.Enabled = enabled;
            EncryptBtn.Enabled = enabled;
            DecryptBtn.Enabled = enabled;
        }

        private void EncryptBtn_Click(object sender, EventArgs e)
        {
            _watch.Reset();
            if (!File.Exists(FilePathTxt.Text))
            {
                MessageBox.Show(Resources.FileNotFoundMessage);
                return;
            }

            ChangeStates(false);
            StatusLbl.Text = Resources.EncryptionInProgressMessage;
            Task.Run(() =>
            {
                _watch.Start();
                ICrypt.BmpEncrypt(File.ReadAllBytes(FilePathTxt.Text))
                    .Save(Path.GetDirectoryName(FilePathTxt.Text) + $"\\encrypted-{DateTime.Now.Ticks}.png");
                _watch.Stop();
                Invoke(new MethodInvoker(() =>
                {
                    ChangeStates(true);
                    StatusLbl.Text = string.Format(Resources.EncryptionSuccessMessage, _watch.ElapsedMilliseconds);
                }));
            });
        }

        private void DecryptBtn_Click(object sender, EventArgs e)
        {
            _watch.Reset();
            if (!File.Exists(FilePathTxt.Text))
            {
                MessageBox.Show(Resources.FileNotFoundMessage);
                return;
            }

            ChangeStates(false);
            StatusLbl.Text = Resources.DecryptionInProgressMessage;

            var filePath =
                Path.GetDirectoryName(FilePathTxt.Text) + $"\\decrypted-{DateTime.Now.Ticks}.{FileExtensionBx.Text}";

            Task.Run(() =>
                {
                    _watch.Start();
                    using (BinaryWriter bw = new BinaryWriter(File.Create(filePath)))
                        bw.Write(ICrypt.BmpDecrypt((Bitmap)Image.FromFile(FilePathTxt.Text)));
                    _watch.Stop();
                    Invoke(new MethodInvoker(() =>
                    {
                        ChangeStates(true);
                        StatusLbl.Text = string.Format(Resources.DecryptionSuccessMessage, _watch.ElapsedMilliseconds);
                    }));
                });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<string> s = new List<string> {"a", "b"};
            var image = ICrypt.ImageFromObject(s);
            image.Save("lmao.png");
            var newList = ICrypt.ImageToObject<List<string>>((Bitmap)Image.FromFile("lmao.png"));
            MessageBox.Show(newList[1]);
        }
    }
}
