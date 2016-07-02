using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
        string deviceID = "";

        int infoPointer = 0;
        int headerPointer = 0;
        int dataPointer = 0;
        int powerPointer = 0;
        int limitOffset = 0;
        int maxFreqOffset = 0x17;
        int vidOffset = 0;
        int dpmOffset = 0;
        int memOffset = 0;
        int dpmEntryOffset = 0x04;
        int dpmEntrySize = 0x0F; // 0x0B - Fiji, 0x0F - Polaris
        int dpmEntryCount = 0;
        int vidEntryOffset = 0x01;
        int vidEntrySize = 0x08;
        int vidEntryCount = 0;
        int memEntryOffset = 0x06;
        int memEntrySize = 0x0D;
        int memEntryCount = 0;


        public MainWindow()
        {
            InitializeComponent();
            MainWindow.GetWindow(this).Title += " 1.1";

            save.IsEnabled = false;
            boxInfo.IsEnabled = false;
            boxPower.IsEnabled = false;
            boxGPU.IsEnabled = false;
            boxMem.IsEnabled = false;

            MessageBox.Show("Modifying your BIOS is dangerous and could cause irreversible damage to your GPU.\nUsing a modified BIOS may void your warranty.\nThe author will not be held accountable for your actions.", "DISCLAIMER", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void OpenFileDialog_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "BIOS (.rom)|*.rom|All Files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == true) {
                save.IsEnabled = false;

                txtTDP.Clear();
                txtTDC.Clear();
                txtMPDL.Clear();
                txtThrottleTemp.Clear();
                txtMaxGPU.Clear();
                txtMaxMEM.Clear();
                tableGPU.Items.Clear();
                tableMEM.Items.Clear();

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

                    vidOffset = getValueAtPosition(16, powerPointer + 0x31) + 0x01;
                    vidEntryCount = getValueAtPosition(8, powerPointer + vidOffset);

                    dpmOffset = getValueAtPosition(16, powerPointer + 0x2D) + 0x01;
                    dpmEntryCount = getValueAtPosition(8, powerPointer + dpmOffset);

                    memOffset = getValueAtPosition(16, powerPointer + 0x2B) + 0x01;
                    memEntryCount = getValueAtPosition(8, powerPointer + memOffset);

                    deviceID = getValueAtPosition(16, infoPointer + 0x06).ToString("X");
                    txtDeviceID.Text = "0x" + deviceID;
                    txtVendorID.Text = "0x" + getValueAtPosition(16, infoPointer + 0x04).ToString("X");
                    txtChecksum.Text = "0x" + getValueAtPosition(8, 0x21).ToString("X");

                    if (!supportedDeviceID.Contains(deviceID)) {
                        MessageBox.Show("Unsupported BIOS (0x" + deviceID + ")", "WARNING", MessageBoxButton.OK, MessageBoxImage.Warning);
                    } else if (dpmEntryCount != vidEntryCount) {
                        MessageBox.Show("Invalid DPM/VID entries!", "WARNING", MessageBoxButton.OK, MessageBoxImage.Warning);
                    } else {
                        txtTDP.Text = getValueAtPosition(16, powerPointer + limitOffset).ToString();
                        txtTDC.Text = getValueAtPosition(16, powerPointer + limitOffset + 0x04).ToString();
                        txtMPDL.Text = getValueAtPosition(16, powerPointer + limitOffset + 0x0E).ToString();
                        txtThrottleTemp.Text = getValueAtPosition(16, powerPointer + limitOffset + 0x10).ToString();

                        txtMaxGPU.Text = getValueAtPosition(24, powerPointer + maxFreqOffset, true).ToString();
                        txtMaxMEM.Text = getValueAtPosition(24, powerPointer + maxFreqOffset + 0x04, true).ToString();

                        tableGPU.Items.Clear();
                        for (var i = 0; i < dpmEntryCount; i++) {
                            tableGPU.Items.Add(new {
                                DPM = getValueAtPosition(24, powerPointer + dpmOffset + dpmEntryOffset + dpmEntrySize*i, true).ToString(),
                                VID = getValueAtPosition(16, powerPointer + vidOffset + vidEntryOffset + vidEntrySize*i).ToString()
                            });
                        }

                        tableMEM.Items.Clear();
                        for (var i = 0; i < memEntryCount; i++) {
                            tableMEM.Items.Add(new
                            {
                                DPM = getValueAtPosition(24, powerPointer + memOffset + memEntryOffset + memEntrySize*i + 0x02, true).ToString(),
                                VID = getValueAtPosition(16, powerPointer + memOffset + memEntryOffset + memEntrySize*i).ToString()
                            });
                        }

                        save.IsEnabled = true;
                        boxInfo.IsEnabled = true;
                        boxPower.IsEnabled = true;
                        boxGPU.IsEnabled = true;
                        boxMem.IsEnabled = true;
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

                for (var i = 0; i < dpmEntryCount; i++) {
                    var container = tableGPU.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                    var dpm = FindByName("DPM", container) as TextBox;
                    var vid = FindByName("VID", container) as TextBox;

                    setValueAtPosition(dpm.Text, 24, powerPointer + dpmOffset + dpmEntryOffset + dpmEntrySize*i, true);
                    setValueAtPosition(vid.Text, 16, powerPointer + vidOffset + vidEntryOffset + vidEntrySize*i);
                }

                for (var i = 0; i < memEntryCount; i++) {
                    var container = tableMEM.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                    var dpm = FindByName("DPM", container) as TextBox;
                    var vid = FindByName("VID", container) as TextBox;

                    setValueAtPosition(dpm.Text, 24, powerPointer + memOffset + memEntryOffset + memEntrySize * i + 0x02, true);
                    setValueAtPosition(vid.Text, 16, powerPointer + memOffset + memEntryOffset + memEntrySize * i);
                }

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

        private FrameworkElement FindByName(string name, FrameworkElement root)
        {
            Stack<FrameworkElement> tree = new Stack<FrameworkElement>();
            tree.Push(root);

            while (tree.Count > 0)
            {
                FrameworkElement current = tree.Pop();
                if (current.Name == name)
                    return current;

                int count = VisualTreeHelper.GetChildrenCount(current);
                for (int i = 0; i < count; ++i)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(current, i);
                    if (child is FrameworkElement)
                        tree.Push((FrameworkElement)child);
                }
            }

            return null;
        }
    }
}
