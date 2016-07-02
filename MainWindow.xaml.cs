using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PolarisBiosEditor
{
    public partial class MainWindow : Window
    {
        Byte[] buffer;
        string[] supportedDeviceID = new string[] { "67DF" };

        int infoPointer = 0;
        int headerPointer = 0;
        int dataPointer = 0;
        int powerPointer = 0;
        int limitOffset = 0;
        int maxFreqOffset = 0;
        int vidOffset = 0;
        int dpmOffset = 0;

        public MainWindow()
        {
            InitializeComponent();
            MainWindow.GetWindow(this).Title += " 1.0";

            save.IsEnabled = false;
            txtDeviceID.IsEnabled = false;
            txtVendorID.IsEnabled = false;
            txtChecksum.IsReadOnly = true;

            txtTDP.IsEnabled = false;
            txtTDC.IsEnabled = false;
            txtMPDL.IsEnabled = false;
            txtThrottleTemp.IsEnabled = false;

            txtMaxGPU.IsEnabled = false;
            txtMaxMEM.IsEnabled = false;

            txtDPM0.IsEnabled = false;
            txtVID0.IsEnabled = false;
            txtDPM1.IsEnabled = false;
            txtVID1.IsEnabled = false;
            txtDPM2.IsEnabled = false;
            txtVID2.IsEnabled = false;
            txtDPM3.IsEnabled = false;
            txtVID3.IsEnabled = false;
            txtDPM4.IsEnabled = false;
            txtVID4.IsEnabled = false;
            txtDPM5.IsEnabled = false;
            txtVID5.IsEnabled = false;
            txtDPM6.IsEnabled = false;
            txtVID6.IsEnabled = false;
            txtDPM7.IsEnabled = false;
            txtVID7.IsEnabled = false;

            MessageBox.Show("Modifying your BIOS is dangerous and could cause irreversible damage to your GPU.\nUsing a modified BIOS may void your warranty.\nThe author will not be held accountable for your actions.", "DISCLAIMER", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void OpenFileDialog_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "BIOS (.rom)|*.rom|All Files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == true) {
                System.IO.Stream fileStream = openFileDialog.OpenFile();

                using (BinaryReader br = new BinaryReader(fileStream)) {
                    buffer = br.ReadBytes((int)fileStream.Length);
                    fixChecksum(false);

                    infoPointer = getValueAtPosition(16, 0x18);
                    headerPointer = getValueAtPosition(16, 0x48);
                    dataPointer = getValueAtPosition(16, headerPointer + 0x20);
                    powerPointer = getValueAtPosition(16, dataPointer + 0x22);
                    limitOffset = getValueAtPosition(16, powerPointer + 0x39) + 0x01;
                    maxFreqOffset = 0x17;
                    vidOffset = getValueAtPosition(16, powerPointer + 0x31) + 0x02;
                    dpmOffset = getValueAtPosition(16, powerPointer + 0x2D) + 0x05;

                    var deviceID = getValueAtPosition(16, infoPointer + 0x06).ToString("X");
                    txtDeviceID.Text = "0x" + deviceID;
                    txtVendorID.Text = "0x" + getValueAtPosition(16, infoPointer + 0x04).ToString("X");
                    txtChecksum.Text = "0x" + getValueAtPosition(8, 0x21).ToString("X");

                    if (!supportedDeviceID.Contains(deviceID)) {
                        save.IsEnabled = false;
                        MessageBox.Show("Unsupported BIOS (0x" + deviceID + ")", "WARNING", MessageBoxButton.OK, MessageBoxImage.Warning);
                    } else {
                        txtTDP.Text = getValueAtPosition(16, powerPointer + limitOffset).ToString();
                        txtTDC.Text = getValueAtPosition(16, powerPointer + limitOffset + 0x04).ToString();
                        txtMPDL.Text = getValueAtPosition(16, powerPointer + limitOffset + 0x0E).ToString();
                        txtThrottleTemp.Text = getValueAtPosition(16, powerPointer + limitOffset + 0x10).ToString();

                        txtMaxGPU.Text = getValueAtPosition(24, powerPointer + maxFreqOffset, true).ToString();
                        txtMaxMEM.Text = getValueAtPosition(24, powerPointer + maxFreqOffset + 0x04, true).ToString();

                        txtDPM0.Text = getValueAtPosition(24, powerPointer + dpmOffset, true).ToString();
                        txtDPM1.Text = getValueAtPosition(24, powerPointer + dpmOffset + 0x0F, true).ToString();
                        txtDPM2.Text = getValueAtPosition(24, powerPointer + dpmOffset + 0x0F*2, true).ToString();
                        txtDPM3.Text = getValueAtPosition(24, powerPointer + dpmOffset + 0x0F*3, true).ToString();
                        txtDPM4.Text = getValueAtPosition(24, powerPointer + dpmOffset + 0x0F*4, true).ToString();
                        txtDPM5.Text = getValueAtPosition(24, powerPointer + dpmOffset + 0x0F*5, true).ToString();
                        txtDPM6.Text = getValueAtPosition(24, powerPointer + dpmOffset + 0x0F*6, true).ToString();
                        txtDPM7.Text = getValueAtPosition(24, powerPointer + dpmOffset + 0x0F*7, true).ToString();

                        txtVID0.Text = getValueAtPosition(16, powerPointer + vidOffset).ToString();
                        txtVID1.Text = getValueAtPosition(16, powerPointer + vidOffset + 0x08).ToString();
                        txtVID2.Text = getValueAtPosition(16, powerPointer + vidOffset + 0x08*2).ToString();
                        txtVID3.Text = getValueAtPosition(16, powerPointer + vidOffset + 0x08*3).ToString();
                        txtVID4.Text = getValueAtPosition(16, powerPointer + vidOffset + 0x08*4).ToString();
                        txtVID5.Text = getValueAtPosition(16, powerPointer + vidOffset + 0x08*5).ToString();
                        txtVID6.Text = getValueAtPosition(16, powerPointer + vidOffset + 0x08*6).ToString();
                        txtVID7.Text = getValueAtPosition(16, powerPointer + vidOffset + 0x08*7).ToString();

                        save.IsEnabled = true;
                        txtDeviceID.IsEnabled = true;
                        txtVendorID.IsEnabled = true;

                        txtTDP.IsEnabled = true;
                        txtTDC.IsEnabled = true;
                        txtMPDL.IsEnabled = true;
                        txtThrottleTemp.IsEnabled = true;

                        txtMaxGPU.IsEnabled = true;
                        txtMaxMEM.IsEnabled = true;

                        txtDPM0.IsEnabled = true;
                        txtVID0.IsEnabled = true;
                        txtDPM1.IsEnabled = true;
                        txtVID1.IsEnabled = true;
                        txtDPM2.IsEnabled = true;
                        txtVID2.IsEnabled = true;
                        txtDPM3.IsEnabled = true;
                        txtVID3.IsEnabled = true;
                        txtDPM4.IsEnabled = true;
                        txtVID4.IsEnabled = true;
                        txtDPM5.IsEnabled = true;
                        txtVID5.IsEnabled = true;
                        txtDPM6.IsEnabled = true;
                        txtVID6.IsEnabled = true;
                        txtDPM7.IsEnabled = true;
                        txtVID7.IsEnabled = true;
                    }
                    fileStream.Close();
                }
            }
        }

        public Int32 getValueAtPosition(int bits, int position, bool isFrequency = false)
        {
            int value = 0;
            if (position <= buffer.Length - 4) {
                switch (bits) {
                    case 8:
                    default:
                        value = buffer[position];
                        break;
                    case 16:
                        value = (buffer[position + 1] << 8) | buffer[position];
                        break;
                    case 24:
                        value = (buffer[position + 2] << 16) | (buffer[position + 1] << 8) | buffer[position];
                        break;
                    case 32:
                        value = (buffer[position + 3] << 24) | (buffer[position + 2] << 16) | (buffer[position + 1] << 8) | buffer[position];
                        break;
                }
                if (isFrequency) return value / 100;
                return value;
            }
            return -1;
        }

        public bool setValueAtPosition(int value, int bits, int position, bool isFrequency = false)
        {
            if (isFrequency) value *= 100;
            if (position <= buffer.Length - 4) {
                switch (bits) {
                    case 8:
                    default:
                        buffer[position] = (byte)value;
                        break;
                    case 16:
                        buffer[position] = (byte)value;
                        buffer[position + 1] = (byte)(value >> 8);
                        break;
                    case 24:
                        buffer[position] = (byte)value;
                        buffer[position + 1] = (byte)(value >> 8);
                        buffer[position + 2] = (byte)(value >> 16);
                        break;
                    case 32:
                        buffer[position] = (byte)value;
                        buffer[position + 1] = (byte)(value >> 8);
                        buffer[position + 2] = (byte)(value >> 16);
                        buffer[position + 3] = (byte)(value >> 32);
                        break;
                }
                return true;
            }
            return false;
        }

        private bool setValueAtPosition(String text, int bits, int position, bool isFrequency = false)
        {
            int value = 0;
            if (!int.TryParse(text, out value)) {
                return false;
            }
            return setValueAtPosition(value, bits, position, isFrequency);
        }

        private void SaveFileDialog_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog SaveFileDialog = new SaveFileDialog();
            SaveFileDialog.Title = "Save As";
            SaveFileDialog.Filter = "BIOS (*.rom)|*.rom";

            if (SaveFileDialog.ShowDialog() == true) {
                FileStream fs = new FileStream(SaveFileDialog.FileName, FileMode.Create);
                BinaryWriter bw = new BinaryWriter(fs);

                setValueAtPosition(txtDeviceID.Text, 16, infoPointer + 0x06);
                setValueAtPosition(txtVendorID.Text, 16, infoPointer + 0x04);

                setValueAtPosition(txtTDP.Text, 16, powerPointer + limitOffset);
                setValueAtPosition(txtTDC.Text, 16, powerPointer + limitOffset + 0x04);
                setValueAtPosition(txtMPDL.Text, 16, powerPointer + limitOffset + 0x0E);
                setValueAtPosition(txtThrottleTemp.Text, 16, powerPointer + limitOffset + 0x10);

                setValueAtPosition(txtMaxGPU.Text, 24, powerPointer + maxFreqOffset, true);
                setValueAtPosition(txtMaxMEM.Text, 24, powerPointer + maxFreqOffset + 0x04, true);

                setValueAtPosition(txtDPM0.Text, 24, powerPointer + dpmOffset, true);
                setValueAtPosition(txtDPM1.Text, 24, powerPointer + dpmOffset + 0x0F, true);
                setValueAtPosition(txtDPM2.Text, 24, powerPointer + dpmOffset + 0x0F*2, true);
                setValueAtPosition(txtDPM3.Text, 24, powerPointer + dpmOffset + 0x0F*3, true);
                setValueAtPosition(txtDPM4.Text, 24, powerPointer + dpmOffset + 0x0F*4, true);
                setValueAtPosition(txtDPM5.Text, 24, powerPointer + dpmOffset + 0x0F*5, true);
                setValueAtPosition(txtDPM6.Text, 24, powerPointer + dpmOffset + 0x0F*6, true);
                setValueAtPosition(txtDPM7.Text, 24, powerPointer + dpmOffset + 0x0F*7, true);

                setValueAtPosition(txtVID0.Text, 16, powerPointer + vidOffset);
                setValueAtPosition(txtVID1.Text, 16, powerPointer + vidOffset + 0x08);
                setValueAtPosition(txtVID2.Text, 16, powerPointer + vidOffset + 0x08*2);
                setValueAtPosition(txtVID3.Text, 16, powerPointer + vidOffset + 0x08*3);
                setValueAtPosition(txtVID4.Text, 16, powerPointer + vidOffset + 0x08*4);
                setValueAtPosition(txtVID5.Text, 16, powerPointer + vidOffset + 0x08*5);
                setValueAtPosition(txtVID6.Text, 16, powerPointer + vidOffset + 0x08*6);
                setValueAtPosition(txtVID7.Text, 16, powerPointer + vidOffset + 0x08*7);

                fixChecksum(true);
                bw.Write(buffer);

                fs.Close();
                bw.Close();
            }
        }

        private void fixChecksum(bool save)
        {
            Byte checksum = buffer[0x21];
            int size = buffer[0x02] * 512;
            Byte offset = 0;

            for (int i = 0; i < size; i++) {
                offset += buffer[i];
            }
            if (checksum == (buffer[0x21] - offset)) {
                txtChecksum.Foreground = Brushes.Green;
            } else {
                txtChecksum.Foreground = Brushes.Red;
                MessageBox.Show("Invalid checksum - Save to fix!", "WARNING", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            if (save) {
                buffer[0x21] -= offset;
                txtChecksum.Text = "0x" + getValueAtPosition(8, 0x21).ToString("X");
                txtChecksum.Foreground = Brushes.Green;
            }
        }
    }
}
