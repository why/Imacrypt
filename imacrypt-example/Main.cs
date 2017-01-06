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

namespace imacrypt_example
{
    public partial class Main : Form
    {
        private Stopwatch _watch = new Stopwatch();
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
                MessageBox.Show("This file doesn't exist!");
                return;
            }

            ChangeStates(false);
            StatusLbl.Text = "Encrypting..";
            Task.Run(() =>
            {
                _watch.Start();
                File.ReadAllBytes(FilePathTxt.Text)
                .BmpEncrypt()
                .Save(Path.GetDirectoryName(FilePathTxt.Text) + $"\\encrypted-{DateTime.Now.Ticks}.png");
                _watch.Stop();
                Invoke(new MethodInvoker(() =>
                {
                    ChangeStates(true);
                    StatusLbl.Text = $"Encrypted file in {_watch.ElapsedMilliseconds}ms!";
                }));
            });
        }

        private void DecryptBtn_Click(object sender, EventArgs e)
        {
            _watch.Reset();
            if (!File.Exists(FilePathTxt.Text))
            {
                MessageBox.Show("This file doesn't exist!");
                return;
            }

            ChangeStates(false);
            StatusLbl.Text = "Decrypting..";

            var filePath =
                Path.GetDirectoryName(FilePathTxt.Text) + $"\\decrypted-{DateTime.Now.Ticks}.{FileExtensionBx.Text}";

            Task.Run(() =>
                {
                    _watch.Start();
                    using (BinaryWriter bw = new BinaryWriter(File.Create(filePath)))
                        bw.Write(((Bitmap)Image.FromFile(FilePathTxt.Text)).BmpDecrypt());
                    _watch.Stop();
                    Invoke(new MethodInvoker(() =>
                    {
                        ChangeStates(true);
                        StatusLbl.Text = $"Decrypted file in {_watch.ElapsedMilliseconds}ms!";
                    }));
                });
        }
    }
}
