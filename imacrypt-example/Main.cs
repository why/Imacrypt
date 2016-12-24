using Imacrypt;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
            if (!File.Exists(FilePathTxt.Text))
            {
                MessageBox.Show("This file doesn't exist!");
                return;
            }

            ChangeStates(false);
            StatusLbl.Text = "Encrypting..";
            new Thread(() =>
            {
                File.ReadAllBytes(FilePathTxt.Text)
                .BmpEncrypt()
                .Save(Path.GetDirectoryName(FilePathTxt.Text) + $"\\encrypted-{DateTime.Now.Ticks}.png");
                Invoke(new MethodInvoker(() =>
                {
                    ChangeStates(true);
                    StatusLbl.Text = "Idle";
                }));
            }).Start();
        }

        private void DecryptBtn_Click(object sender, EventArgs e)
        {
            if (!File.Exists(FilePathTxt.Text))
            {
                MessageBox.Show("This file doesn't exist!");
                return;
            }

            ChangeStates(false);
            StatusLbl.Text = "Decrypting..";
            new Thread(() =>
            {
                using (BinaryWriter bw = new BinaryWriter(
                    File.Create(
                        Path.GetDirectoryName(FilePathTxt.Text)
                        + $"\\decrypted-{DateTime.Now.Ticks}.{1}")))
                    bw.Write(((Bitmap)Image.FromFile(FilePathTxt.Text)).BmpDecrypt());
            }).Start();
        }
    }
}
