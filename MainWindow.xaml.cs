using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PolarisBiosEditor
{
    public partial class MainWindow : Window
    {
        Byte[] buffer;
        Int32Converter int32 = new Int32Converter();
        string[] supportedDeviceID = new string[] { "67DF" };
        string deviceID = "";

        int atom_rom_checksum_offset = 0x21;
        int atom_rom_header_ptr = 0x48;
        int atom_rom_header_offset;
        ATOM_ROM_HEADER atom_rom_header;
        ATOM_DATA_TABLES atom_data_table;

        int atom_powerplay_offset;
        ATOM_POWERPLAY_TABLE atom_powerplay_table;

        int atom_powertune_offset;
        ATOM_POWERTUNE_TABLE atom_powertune_table;

        int atom_fan_offset;
        ATOM_FAN_TABLE atom_fan_table;

        int atom_mclk_table_offset;
        ATOM_MCLK_TABLE atom_mclk_table;
        ATOM_MCLK_ENTRY[] atom_mclk_entries;

        int atom_sclk_table_offset;
        ATOM_SCLK_TABLE atom_sclk_table;
        ATOM_SCLK_ENTRY[] atom_sclk_entries;

        int atom_vddc_table_offset;
        ATOM_VOLTAGE_TABLE atom_vddc_table;
        ATOM_VOLTAGE_ENTRY[] atom_vddc_entries;

        int atom_vram_info_offset;
        ATOM_VRAM_INFO atom_vram_info;
        ATOM_VRAM_ENTRY[] atom_vram_entries;
        ATOM_VRAM_TIMING_ENTRY[] atom_vram_timing_entries;
        int atom_vram_index = 0;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct ATOM_COMMON_TABLE_HEADER
        {
            Int16 usStructureSize;
            Byte ucTableFormatRevision;
            Byte ucTableContentRevision;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct ATOM_ROM_HEADER
        {
            public ATOM_COMMON_TABLE_HEADER sHeader;
            public UInt32 uaFirmWareSignature;
            public UInt16 usBiosRuntimeSegmentAddress;
            public UInt16 usProtectedModeInfoOffset;
            public UInt16 usConfigFilenameOffset;
            public UInt16 usCRC_BlockOffset;
            public UInt16 usBIOS_BootupMessageOffset;
            public UInt16 usInt10Offset;
            public UInt16 usPciBusDevInitCode;
            public UInt16 usIoBaseAddress;
            public UInt16 usSubsystemVendorID;
            public UInt16 usSubsystemID;
            public UInt16 usPCI_InfoOffset;
            public UInt16 usMasterCommandTableOffset;
            public UInt16 usMasterDataTableOffset;
            public Byte ucExtendedFunctionCode;
            public Byte ucReserved;
            public UInt32 ulPSPDirTableOffset;
            public UInt16 usVendorID;
            public UInt16 usDeviceID;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct ATOM_DATA_TABLES
        {
            public ATOM_COMMON_TABLE_HEADER sHeader;
            public UInt16 UtilityPipeLine;
            public UInt16 MultimediaCapabilityInfo;
            public UInt16 MultimediaConfigInfo;
            public UInt16 StandardVESA_Timing;
            public UInt16 FirmwareInfo;
            public UInt16 PaletteData;
            public UInt16 LCD_Info;
            public UInt16 DIGTransmitterInfo;
            public UInt16 SMU_Info;
            public UInt16 SupportedDevicesInfo;
            public UInt16 GPIO_I2C_Info;
            public UInt16 VRAM_UsageByFirmware;
            public UInt16 GPIO_Pin_LUT;
            public UInt16 VESA_ToInternalModeLUT;
            public UInt16 GFX_Info;
            public UInt16 PowerPlayInfo;
            public UInt16 GPUVirtualizationInfo;
            public UInt16 SaveRestoreInfo;
            public UInt16 PPLL_SS_Info;
            public UInt16 OemInfo;
            public UInt16 XTMDS_Info;
            public UInt16 MclkSS_Info;
            public UInt16 Object_Header;
            public UInt16 IndirectIOAccess;
            public UInt16 MC_InitParameter;
            public UInt16 ASIC_VDDC_Info;
            public UInt16 ASIC_InternalSS_Info;
            public UInt16 TV_VideoMode;
            public UInt16 VRAM_Info;
            public UInt16 MemoryTrainingInfo;
            public UInt16 IntegratedSystemInfo;
            public UInt16 ASIC_ProfilingInfo;
            public UInt16 VoltageObjectInfo;
            public UInt16 PowerSourceInfo;
            public UInt16 ServiceInfo;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct ATOM_POWERPLAY_TABLE
        {
            public ATOM_COMMON_TABLE_HEADER sHeader;
            public Byte ucTableRevision;
            public UInt16 usTableSize;
            public UInt32 ulGoldenPPID;
            public UInt32 ulGoldenRevision;
            public UInt16 usFormatID;
            public UInt16 usVoltageTime;
            public UInt32 ulPlatformCaps;
            public UInt32 ulMaxODEngineClock;
            public UInt32 ulMaxODMemoryClock;
            public UInt16 usPowerControlLimit;
            public UInt16 usUlvVoltageOffset;
            public UInt16 usStateArrayOffset;
            public UInt16 usFanTableOffset;
            public UInt16 usThermalControllerOffset;
            public UInt16 usReserv;
            public UInt16 usMclkDependencyTableOffset;
            public UInt16 usSclkDependencyTableOffset;
            public UInt16 usVddcLookupTableOffset;
            public UInt16 usVddgfxLookupTableOffset;
            public UInt16 usMMDependencyTableOffset;
            public UInt16 usVCEStateTableOffset;
            public UInt16 usPPMTableOffset;
            public UInt16 usPowerTuneTableOffset;
            public UInt16 usHardLimitTableOffset;
            public UInt16 usPCIETableOffset;
            public UInt16 usGPIOTableOffset;
            public fixed UInt16 usReserved[6];
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct ATOM_MCLK_ENTRY
        {
            public Byte ucVddcInd;
            public UInt16 usVddci;
            public UInt16 usVddgfxOffset;
            public UInt16 usMvdd;
            public UInt32 ulMclk;
            public UInt16 usReserved;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct ATOM_MCLK_TABLE
        {
            public Byte ucRevId;
            public Byte ucNumEntries;
            // public ATOM_MCLK_ENTRY entries[ucNumEntries];
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct ATOM_SCLK_ENTRY
        {
            public Byte ucVddInd;
            public UInt16 usVddcOffset;
            public UInt32 ulSclk;
            public UInt16 usEdcCurrent;
            public Byte ucReliabilityTemperature;
            public Byte ucCKSVOffsetandDisable;
            public UInt32 ulSclkOffset; // Polaris Only, remove for compatibility with Fiji
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct ATOM_SCLK_TABLE
        {
            public Byte ucRevId;
            public Byte ucNumEntries;
            // public ATOM_SCLK_ENTRY entries[ucNumEntries];
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct ATOM_VOLTAGE_ENTRY
        {
            public UInt16 usVdd;
            public UInt16 usCACLow;
            public UInt16 usCACMid;
            public UInt16 usCACHigh;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct ATOM_VOLTAGE_TABLE
        {
            public Byte ucRevId;
            public Byte ucNumEntries;
            // public ATOM_VOLTAGE_ENTRY entries[ucNumEntries];
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct ATOM_FAN_TABLE
        {
            public Byte ucRevId;
            public Byte ucTHyst;
            public UInt16 usTMin;
            public UInt16 usTMed;
            public UInt16 usTHigh;
            public UInt16 usPWMMin;
            public UInt16 usPWMMed;
            public UInt16 usPWMHigh;
            public UInt16 usTMax;
            public Byte ucFanControlMode;
            public UInt16 usFanPWMMax;
            public UInt16 usFanOutputSensitivity;
            public UInt16 usFanRPMMax;
            public UInt32 ulMinFanSCLKAcousticLimit;
            public Byte ucTargetTemperature;
            public Byte ucMinimumPWMLimit;
            public UInt16 usFanGainEdge;
            public UInt16 usFanGainHotspot;
            public UInt16 usFanGainLiquid;
            public UInt16 usFanGainVrVddc;
            public UInt16 usFanGainVrMvdd;
            public UInt16 usFanGainPlx;
            public UInt16 usFanGainHbm;
            public UInt16 usReserved;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct ATOM_POWERTUNE_TABLE
        {
            public Byte ucRevId;
            public UInt16 usTDP;
            public UInt16 usConfigurableTDP;
            public UInt16 usTDC;
            public UInt16 usBatteryPowerLimit;
            public UInt16 usSmallPowerLimit;
            public UInt16 usLowCACLeakage;
            public UInt16 usHighCACLeakage;
            public UInt16 usMaximumPowerDeliveryLimit;
            public UInt16 usTjMax;
            public UInt16 usPowerTuneDataSetID;
            public UInt16 usEDCLimit;
            public UInt16 usSoftwareShutdownTemp;
            public UInt16 usClockStretchAmount;
            public UInt16 usTemperatureLimitHotspot;
            public UInt16 usTemperatureLimitLiquid1;
            public UInt16 usTemperatureLimitLiquid2;
            public UInt16 usTemperatureLimitVrVddc;
            public UInt16 usTemperatureLimitVrMvdd;
            public UInt16 usTemperatureLimitPlx;
            public Byte ucLiquid1_I2C_address;
            public Byte ucLiquid2_I2C_address;
            public Byte ucLiquid_I2C_Line;
            public Byte ucVr_I2C_address;
            public Byte ucVr_I2C_Line;
            public Byte ucPlx_I2C_address;
            public Byte ucPlx_I2C_Line;
            public UInt16 usReserved;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct ATOM_VRAM_TIMING_ENTRY {
            public UInt32 ulClkRange;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x30)]
            public Byte[] ucLatency;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct ATOM_VRAM_ENTRY
        {
            public UInt32 ulChannelMapCfg;
            public UInt16 usModuleSize;
            public UInt16 usMcRamCfg;
            public UInt16 usEnableChannels;
            public Byte ucExtMemoryID;
            public Byte ucMemoryType;
            public Byte ucChannelNum;
            public Byte ucChannelWidth;
            public Byte ucDensity;
            public Byte ucBankCol;
            public Byte ucMisc;
            public Byte ucVREFI;
            public UInt16 usReserved;
            public UInt16 usMemorySize;
            public Byte ucMcTunningSetId;
            public Byte ucRowNum;
            public UInt16 usEMRS2Value;
            public UInt16 usEMRS3Value;
            public Byte ucMemoryVenderID;
            public Byte ucRefreshRateFactor;
            public Byte ucFIFODepth;
            public Byte ucCDR_Bandwidth;
            public UInt32 ulChannelMapCfg1;
            public UInt32 ulBankMapCfg;
            public UInt32 ulReserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public Byte[] strMemPNString;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct ATOM_VRAM_INFO
        {
            public ATOM_COMMON_TABLE_HEADER sHeader;
            public UInt16 usMemAdjustTblOffset;
            public UInt16 usMemClkPatchTblOffset;
            public UInt16 usMcAdjustPerTileTblOffset;
            public UInt16 usMcPhyInitTableOffset;
            public UInt16 usDramDataRemapTblOffset;
            public UInt16 usReserved1;
            public Byte ucNumOfVRAMModule;
            public Byte ucMemoryClkPatchTblVer;
            public Byte ucVramModuleVer;
            public Byte ucMcPhyTileNum;
            // public ATOM_VRAM_ENTRY aVramInfo[ucNumOfVRAMModule];
        }

        static byte[] getBytes(object obj)
        {
            int size = Marshal.SizeOf(obj);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(obj, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }

        static T fromBytes<T>(byte[] arr)
        {
            T obj = default(T);
            int size = Marshal.SizeOf(obj);
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(arr, 0, ptr, size);
            obj = (T)Marshal.PtrToStructure(ptr, obj.GetType());
            Marshal.FreeHGlobal(ptr);

            return obj;
        }

        public string GetPropertyName<T>(Expression<Func<T>> propertyLambda)
        {
            var me = propertyLambda.Body as MemberExpression;
            if (me == null) {
                throw new ArgumentException();
            }
            return me.Member.Name;
        }

        public void setBytesAtPosition(byte[] dest, int ptr, byte[] src)
        {
            for (var i = 0; i < src.Length; i++) {
                dest[ptr + i] = src[i];
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            MainWindow.GetWindow(this).Title += " 1.3";

            save.IsEnabled = false;
            boxROM.IsEnabled = false;
            boxPOWERPLAY.IsEnabled = false;
            boxPOWERTUNE.IsEnabled = false;
            boxFAN.IsEnabled = false;
            boxGPU.IsEnabled = false;
            boxMEM.IsEnabled = false;
            boxVRAM.IsEnabled = false;

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

                tableROM.Items.Clear();
                tablePOWERPLAY.Items.Clear();
                tablePOWERTUNE.Items.Clear();
                tableFAN.Items.Clear();
                tableGPU.Items.Clear();
                tableMEMORY.Items.Clear();

                System.IO.Stream fileStream = openFileDialog.OpenFile();
                using (BinaryReader br = new BinaryReader(fileStream)) {
                    buffer = br.ReadBytes((int)fileStream.Length);

                    atom_rom_header_offset = getValueAtPosition(16, atom_rom_header_ptr);
                    atom_rom_header = fromBytes<ATOM_ROM_HEADER>(buffer.Skip(atom_rom_header_offset).ToArray());
                    deviceID = atom_rom_header.usDeviceID.ToString("X");

                    fixChecksum(false);

                    if (!supportedDeviceID.Contains(deviceID)) {
                        MessageBox.Show("Unsupported BIOS (0x" + deviceID + ")", "WARNING", MessageBoxButton.OK, MessageBoxImage.Warning);
                    } else {
                        atom_data_table = fromBytes<ATOM_DATA_TABLES>(buffer.Skip(atom_rom_header.usMasterDataTableOffset).ToArray());
                        atom_powerplay_offset = atom_data_table.PowerPlayInfo;
                        atom_powerplay_table = fromBytes<ATOM_POWERPLAY_TABLE>(buffer.Skip(atom_powerplay_offset).ToArray());

                        atom_powertune_offset = atom_data_table.PowerPlayInfo + atom_powerplay_table.usPowerTuneTableOffset;
                        atom_powertune_table = fromBytes<ATOM_POWERTUNE_TABLE>(buffer.Skip(atom_powertune_offset).ToArray());

                        atom_fan_offset = atom_data_table.PowerPlayInfo + atom_powerplay_table.usFanTableOffset;
                        atom_fan_table = fromBytes<ATOM_FAN_TABLE>(buffer.Skip(atom_fan_offset).ToArray());

                        atom_mclk_table_offset = atom_data_table.PowerPlayInfo + atom_powerplay_table.usMclkDependencyTableOffset;
                        atom_mclk_table = fromBytes<ATOM_MCLK_TABLE>(buffer.Skip(atom_mclk_table_offset).ToArray());
                        atom_mclk_entries = new ATOM_MCLK_ENTRY[atom_mclk_table.ucNumEntries];
                        for (var i = 0; i < atom_mclk_entries.Length; i++) {
                            atom_mclk_entries[i] = fromBytes<ATOM_MCLK_ENTRY>(buffer.Skip(atom_mclk_table_offset + Marshal.SizeOf(typeof(ATOM_MCLK_TABLE)) + Marshal.SizeOf(typeof(ATOM_MCLK_ENTRY))*i).ToArray());
                        }

                        atom_sclk_table_offset = atom_data_table.PowerPlayInfo + atom_powerplay_table.usSclkDependencyTableOffset;
                        atom_sclk_table = fromBytes<ATOM_SCLK_TABLE>(buffer.Skip(atom_sclk_table_offset).ToArray());
                        atom_sclk_entries = new ATOM_SCLK_ENTRY[atom_sclk_table.ucNumEntries];
                        for (var i = 0; i < atom_sclk_entries.Length; i++) {
                            atom_sclk_entries[i] = fromBytes<ATOM_SCLK_ENTRY>(buffer.Skip(atom_sclk_table_offset + Marshal.SizeOf(typeof(ATOM_SCLK_TABLE)) + Marshal.SizeOf(typeof(ATOM_SCLK_ENTRY))*i).ToArray());
                        }

                        atom_vddc_table_offset = atom_data_table.PowerPlayInfo + atom_powerplay_table.usVddcLookupTableOffset;
                        atom_vddc_table = fromBytes<ATOM_VOLTAGE_TABLE>(buffer.Skip(atom_vddc_table_offset).ToArray());
                        atom_vddc_entries = new ATOM_VOLTAGE_ENTRY[atom_vddc_table.ucNumEntries];
                        for (var i = 0; i < atom_vddc_table.ucNumEntries; i++) {
                            atom_vddc_entries[i] = fromBytes<ATOM_VOLTAGE_ENTRY>(buffer.Skip(atom_vddc_table_offset + Marshal.SizeOf(typeof(ATOM_VOLTAGE_TABLE)) + Marshal.SizeOf(typeof(ATOM_VOLTAGE_ENTRY))*i).ToArray());
                        }

                        atom_vram_info_offset = atom_data_table.VRAM_Info;
                        atom_vram_info = fromBytes<ATOM_VRAM_INFO>(buffer.Skip(atom_vram_info_offset).ToArray());
                        atom_vram_entries = new ATOM_VRAM_ENTRY[atom_vram_info.ucNumOfVRAMModule];
                        var atom_vram_entry_offset = atom_vram_info_offset + Marshal.SizeOf(typeof(ATOM_VRAM_INFO));
                        for (var i = 0; i < atom_vram_info.ucNumOfVRAMModule; i++) {
                            atom_vram_entries[i] = fromBytes<ATOM_VRAM_ENTRY>(buffer.Skip(atom_vram_entry_offset).ToArray());
                            atom_vram_entry_offset += atom_vram_entries[i].usModuleSize;
                        }
                        atom_vram_timing_entries = new ATOM_VRAM_TIMING_ENTRY[16];
                        for (var i = 0; i < 16; i++) {
                            atom_vram_timing_entries[i] = fromBytes<ATOM_VRAM_TIMING_ENTRY>(buffer.Skip(atom_vram_entry_offset + 0x3D + Marshal.SizeOf(typeof(ATOM_VRAM_TIMING_ENTRY))*i).ToArray());

                            // atom_vram_timing_entries have an undetermined length
                            // attempt to determine the last entry in the array
                            if (atom_vram_timing_entries[i].ulClkRange == 0) {
                                Array.Resize(ref atom_vram_timing_entries, i);
                                break;
                            }
                        }

                        tableROM.Items.Clear();
                        tableROM.Items.Add(new {
                            NAME = "VendorID",
                            VALUE = "0x" + atom_rom_header.usVendorID.ToString("X")
                        });
                        tableROM.Items.Add(new {
                            NAME = "DeviceID",
                            VALUE = "0x" + atom_rom_header.usDeviceID.ToString("X")
                        });
                        tableROM.Items.Add(new {
                            NAME = "Sub ID",
                            VALUE = "0x" + atom_rom_header.usSubsystemID.ToString("X")
                        });
                        tableROM.Items.Add(new {
                            NAME = "Sub VendorID",
                            VALUE = "0x" + atom_rom_header.usSubsystemVendorID.ToString("X")
                        });
                        tableROM.Items.Add(new {
                            NAME = "Firmware Signature",
                            VALUE = "0x" + atom_rom_header.uaFirmWareSignature.ToString("X")
                        });

                        tablePOWERPLAY.Items.Clear();
                        tablePOWERPLAY.Items.Add(new {
                            NAME = "Max GPU Freq. (MHz)",
                            VALUE = atom_powerplay_table.ulMaxODEngineClock / 100
                        });
                        tablePOWERPLAY.Items.Add(new {
                            NAME = "Max Memory Freq. (MHz)",
                            VALUE = atom_powerplay_table.ulMaxODMemoryClock / 100
                        });
                        tablePOWERPLAY.Items.Add(new {
                            NAME = "Power Control Limit (%)",
                            VALUE = atom_powerplay_table.usPowerControlLimit
                        });

                        tablePOWERTUNE.Items.Clear();
                        tablePOWERTUNE.Items.Add(new {
                            NAME = "TDP (W)",
                            VALUE = atom_powertune_table.usTDP
                        });
                        tablePOWERTUNE.Items.Add(new {
                            NAME = "TDC (A)",
                            VALUE = atom_powertune_table.usTDC
                        });
                        tablePOWERTUNE.Items.Add(new {
                            NAME = "Max Power Limit (W)",
                            VALUE = atom_powertune_table.usMaximumPowerDeliveryLimit
                        });
                        tablePOWERTUNE.Items.Add(new {
                            NAME = "Max Temp. (C)",
                            VALUE = atom_powertune_table.usTjMax
                        });
                        tablePOWERTUNE.Items.Add(new {
                            NAME = "Shutdown Temp. (C)",
                            VALUE = atom_powertune_table.usSoftwareShutdownTemp
                        });
                        tablePOWERTUNE.Items.Add(new {
                            NAME = "Hotspot Temp. (C)",
                            VALUE = atom_powertune_table.usTemperatureLimitHotspot
                        });

                        tableFAN.Items.Clear();
                        tableFAN.Items.Add(new {
                            NAME = "Temp. Hysteresis",
                            VALUE = atom_fan_table.ucTHyst
                        });
                        tableFAN.Items.Add(new {
                            NAME = "Min Temp. (C)",
                            VALUE = atom_fan_table.usTMin / 100
                        });
                        tableFAN.Items.Add(new {
                            NAME = "Med Temp. (C)",
                            VALUE = atom_fan_table.usTMed / 100
                        });
                        tableFAN.Items.Add(new {
                            NAME = "High Temp. (C)",
                            VALUE = atom_fan_table.usTHigh / 100
                        });
                        tableFAN.Items.Add(new {
                            NAME = "Max Temp. (C)",
                            VALUE = atom_fan_table.usTMax / 100
                        });
                        tableFAN.Items.Add(new {
                            NAME = "Min PWM (%)",
                            VALUE = atom_fan_table.usPWMMin / 100
                        });
                        tableFAN.Items.Add(new {
                            NAME = "Med PWM (%)",
                            VALUE = atom_fan_table.usPWMMed / 100
                        });
                        tableFAN.Items.Add(new {
                            NAME = "High PWM (%)",
                            VALUE = atom_fan_table.usPWMHigh / 100
                        });
                        tableFAN.Items.Add(new {
                            NAME = "Max PWM (%)",
                            VALUE = atom_fan_table.usFanPWMMax
                        });
                        tableFAN.Items.Add(new {
                            NAME = "Max RPM",
                            VALUE = atom_fan_table.usFanRPMMax
                        });
                        tableFAN.Items.Add(new {
                            NAME = "Sensitivity",
                            VALUE = atom_fan_table.usFanOutputSensitivity
                        });
                        tableFAN.Items.Add(new {
                            NAME = "Acoustic Limit (MHz)",
                            VALUE = atom_fan_table.ulMinFanSCLKAcousticLimit / 100
                        });

                        tableGPU.Items.Clear();
                        for (var i = 0; i < atom_sclk_table.ucNumEntries; i++) {
                            tableGPU.Items.Add(new {
                                MHZ = atom_sclk_entries[i].ulSclk / 100,
                                MV = atom_vddc_entries[atom_sclk_entries[i].ucVddInd].usVdd
                            });
                        }

                        tableMEMORY.Items.Clear();
                        for (var i = 0; i < atom_mclk_table.ucNumEntries; i++) {
                            tableMEMORY.Items.Add(new {
                                MHZ = atom_mclk_entries[i].ulMclk / 100,
                                MV = atom_mclk_entries[i].usMvdd
                            });
                        }

                        listVRAM.Items.Clear();
                        for (var i = 0; i < atom_vram_info.ucNumOfVRAMModule; i++) {
                            listVRAM.Items.Add(Encoding.UTF8.GetString(atom_vram_entries[i].strMemPNString));
                        }
                        listVRAM.SelectedIndex = 0;
                        atom_vram_index = listVRAM.SelectedIndex;

                        tableVRAM_TIMING.Items.Clear();
                        for (var i = 0; i < atom_vram_timing_entries.Length; i++) {
                            tableVRAM_TIMING.Items.Add(new {
                                MHZ = atom_vram_timing_entries[i].ulClkRange / 100,
                                VALUE = ByteArrayToString(atom_vram_timing_entries[i].ucLatency)
                            });
                        }

                        save.IsEnabled = true;
                        boxROM.IsEnabled = true;
                        boxPOWERPLAY.IsEnabled = true;
                        boxPOWERTUNE.IsEnabled = true;
                        boxFAN.IsEnabled = true;
                        boxGPU.IsEnabled = true;
                        boxMEM.IsEnabled = true;
                        boxVRAM.IsEnabled = true;
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

                for (var i = 0; i < tableROM.Items.Count; i++) {
                    var container = tableROM.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                    var name = (FindByName("NAME", container) as TextBlock).Text;
                    var value = (FindByName("VALUE", container) as TextBox).Text;
                    var num = (int)int32.ConvertFromString(value);

                    if (name == "VendorID") {
                        atom_rom_header.usVendorID = (UInt16)num;
                    } else if (name == "DeviceID") {
                        atom_rom_header.usDeviceID = (UInt16)num;
                    } else if (name == "Sub ID") {
                        atom_rom_header.usSubsystemID = (UInt16)num;
                    } else if (name == "Sub VendorID") {
                        atom_rom_header.usSubsystemVendorID = (UInt16)num;
                    } else if (name == "Firmware Signature") {
                        atom_rom_header.uaFirmWareSignature = (UInt32)num;
                    }
                }

                for (var i = 0; i < tablePOWERPLAY.Items.Count; i++) {
                    var container = tablePOWERPLAY.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                    var name = (FindByName("NAME", container) as TextBlock).Text;
                    var value = (FindByName("VALUE", container) as TextBox).Text;
                    var num = (int)int32.ConvertFromString(value);

                    if (name == "Max GPU Freq. (MHz)") {
                        atom_powerplay_table.ulMaxODEngineClock = (UInt32)(num * 100);
                    } else if (name == "Max Memory Freq. (MHz)") {
                        atom_powerplay_table.ulMaxODMemoryClock = (UInt32)(num * 100);
                    } else if (name == "Power Control Limit (%)") {
                        atom_powerplay_table.usPowerControlLimit = (UInt16)num;
                    }
                }

                for (var i = 0; i < tablePOWERTUNE.Items.Count; i++) {
                    var container = tablePOWERTUNE.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                    var name = (FindByName("NAME", container) as TextBlock).Text;
                    var value = (FindByName("VALUE", container) as TextBox).Text;
                    var num = (int)int32.ConvertFromString(value);

                    if (name == "TDP (W)") {
                        atom_powertune_table.usTDP = (UInt16)num;
                    } else if (name == "TDC (A)") {
                        atom_powertune_table.usTDC = (UInt16)num;
                    } else if (name == "Max Power Limit (W)") {
                        atom_powertune_table.usMaximumPowerDeliveryLimit = (UInt16)num;
                    } else if (name == "Max Temp. (C)") {
                        atom_powertune_table.usTjMax = (UInt16)num;
                    } else if (name == "Shutdown Temp. (C)") {
                        atom_powertune_table.usSoftwareShutdownTemp = (UInt16)num;
                    } else if (name == "Hotspot Temp. (C)") {
                        atom_powertune_table.usTemperatureLimitHotspot = (UInt16)num;
                    }
                }

                for (var i = 0; i < tableFAN.Items.Count; i++) {
                    var container = tableFAN.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                    var name = (FindByName("NAME", container) as TextBlock).Text;
                    var value = (FindByName("VALUE", container) as TextBox).Text;
                    var num = (int)int32.ConvertFromString(value);

                    if (name == "Temp. Hysteresis") {
                        atom_fan_table.ucTHyst = (Byte)num;
                    } else if (name == "Min Temp. (C)") {
                        atom_fan_table.usTMin = (UInt16)(num * 100);
                    } else if (name == "Med Temp. (C)") {
                        atom_fan_table.usTMed = (UInt16)(num * 100);
                    } else if (name == "High Temp. (C)") {
                        atom_fan_table.usTHigh = (UInt16)(num * 100);
                    } else if (name == "Max Temp. (C)") {
                        atom_fan_table.usTMax = (UInt16)(num * 100);
                    } else if (name == "Min PWM (%)") {
                        atom_fan_table.usPWMMin = (UInt16)(num * 100);
                    } else if (name == "Med PWM (%)") {
                        atom_fan_table.usPWMMed = (UInt16)(num * 100);
                    } else if (name == "High PWM (%)") {
                        atom_fan_table.usPWMHigh = (UInt16)(num * 100);
                    } else if (name == "Max PWM (%)") {
                        atom_fan_table.usFanPWMMax = (UInt16)num;
                    } else if (name == "Max RPM") {
                        atom_fan_table.usFanRPMMax = (UInt16)num;
                    } else if (name == "Sensitivity") {
                        atom_fan_table.usFanOutputSensitivity = (UInt16)num;
                    } else if (name == "Acoustic Limit (MHz)") {
                        atom_fan_table.ulMinFanSCLKAcousticLimit = (UInt32)(num * 100);
                    }
                }

                for (var i = 0; i < tableGPU.Items.Count; i++) {
                    var container = tableGPU.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                    var mhz = (int)int32.ConvertFromString(((TextBox)FindByName("MHZ", container)).Text) * 100;
                    var mv = (int)int32.ConvertFromString(((TextBox)FindByName("MV", container)).Text);

                    atom_sclk_entries[i].ulSclk = (UInt32)mhz;
                    atom_vddc_entries[atom_sclk_entries[i].ucVddInd].usVdd = (UInt16)mv;
                }

                for (var i = 0; i < tableMEMORY.Items.Count; i++) {
                    var container = tableMEMORY.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                    var mhz = (int)int32.ConvertFromString(((TextBox)FindByName("MHZ", container)).Text) * 100;
                    var mv = (int)int32.ConvertFromString(((TextBox)FindByName("MV", container)).Text);

                    atom_mclk_entries[i].ulMclk = (UInt32)mhz;
                    atom_mclk_entries[i].usMvdd = (UInt16)mv;
                }

                updateVRAM_entries();
                for (var i = 0; i < tableVRAM_TIMING.Items.Count; i++) {
                    var container = tableVRAM_TIMING.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                    var name = (FindByName("MHZ", container) as TextBlock).Text;
                    var value = (FindByName("VALUE", container) as TextBox).Text;
                    var mhz = (int)int32.ConvertFromString(name) * 100;
                    var arr = StringToByteArray(value);

                    atom_vram_timing_entries[i].ulClkRange = (UInt32)mhz;
                    atom_vram_timing_entries[i].ucLatency = arr;
                }

                setBytesAtPosition(buffer, atom_rom_header_offset, getBytes(atom_rom_header));
                setBytesAtPosition(buffer, atom_powerplay_offset, getBytes(atom_powerplay_table));
                setBytesAtPosition(buffer, atom_powertune_offset, getBytes(atom_powertune_table));
                setBytesAtPosition(buffer, atom_fan_offset, getBytes(atom_fan_table));

                for (var i = 0; i < atom_mclk_table.ucNumEntries; i++) {
                    setBytesAtPosition(buffer, atom_mclk_table_offset + Marshal.SizeOf(typeof(ATOM_MCLK_TABLE)) + Marshal.SizeOf(typeof(ATOM_MCLK_ENTRY))*i, getBytes(atom_mclk_entries[i]));
                }

                for (var i = 0; i < atom_sclk_table.ucNumEntries; i++) {
                    setBytesAtPosition(buffer, atom_sclk_table_offset + Marshal.SizeOf(typeof(ATOM_SCLK_TABLE)) + Marshal.SizeOf(typeof(ATOM_SCLK_ENTRY))*i, getBytes(atom_sclk_entries[i]));
                }

                for (var i = 0; i < atom_vddc_table.ucNumEntries; i++) {
                    setBytesAtPosition(buffer, atom_vddc_table_offset + Marshal.SizeOf(typeof(ATOM_VOLTAGE_TABLE)) + Marshal.SizeOf(typeof(ATOM_VOLTAGE_ENTRY))*i, getBytes(atom_vddc_entries[i]));
                }

                var atom_vram_entry_offset = atom_vram_info_offset + Marshal.SizeOf(typeof(ATOM_VRAM_INFO));
                for (var i = 0; i < atom_vram_info.ucNumOfVRAMModule; i++) {
                    setBytesAtPosition(buffer, atom_vram_entry_offset, getBytes(atom_vram_entries[i]));
                    atom_vram_entry_offset += atom_vram_entries[i].usModuleSize;
                }
                for (var i = 0; i < atom_vram_timing_entries.Length; i++) {
                    setBytesAtPosition(buffer, atom_vram_entry_offset + 0x3D + Marshal.SizeOf(typeof(ATOM_VRAM_TIMING_ENTRY))*i, getBytes(atom_vram_timing_entries[i]));
                }

                fixChecksum(true);
                bw.Write(buffer);

                fs.Close();
                bw.Close();
            }
        }

        private void fixChecksum(bool save)
        {
            Byte checksum = buffer[atom_rom_checksum_offset];
            int size = buffer[0x02]*512;
            Byte offset = 0;

            for (int i = 0; i < size; i++) {
                offset += buffer[i];
            }
            if (checksum == (buffer[atom_rom_checksum_offset] - offset)) {
                txtChecksum.Foreground = Brushes.Green;
            } else if (!save) {
                txtChecksum.Foreground = Brushes.Red;
                MessageBox.Show("Invalid checksum - Save to fix!", "WARNING", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            if (save) {
                buffer[atom_rom_checksum_offset] -= offset;
                txtChecksum.Foreground = Brushes.Green;
            }
            txtChecksum.Text = "0x" + buffer[atom_rom_checksum_offset].ToString("X");
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

        public static string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }

        public static byte[] StringToByteArray(String hex)
        {
            if (hex.Length % 2 != 0) {
                MessageBox.Show("Invalid hex string", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw new InvalidDataException();
            }
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2) {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

        public void updateVRAM_entries()
        {
            for (var i = 0; i < tableVRAM.Items.Count; i++) {
                var container = tableVRAM.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                var name = (FindByName("NAME", container) as TextBlock).Text;
                var value = (FindByName("VALUE", container) as TextBox).Text;
                var num = (int)int32.ConvertFromString(value);

                if (name == "VendorID") {
                    atom_vram_entries[atom_vram_index].ucMemoryVenderID = (Byte)num;
                } else if (name == "Size (MB)") {
                    atom_vram_entries[atom_vram_index].usMemorySize = (UInt16)num;
                } else if (name == "Density") {
                    atom_vram_entries[atom_vram_index].ucDensity = (Byte)num;
                } else if (name == "Type") {
                    atom_vram_entries[atom_vram_index].ucMemoryType = (Byte)num;
                }
            }
        }

        private void listVRAM_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            updateVRAM_entries();
            tableVRAM.Items.Clear();
            if (listVRAM.SelectedIndex >= 0 && listVRAM.SelectedIndex < listVRAM.Items.Count) {
                atom_vram_index = listVRAM.SelectedIndex;
                tableVRAM.Items.Add(new {
                    NAME = "VendorID",
                    VALUE = "0x" + atom_vram_entries[atom_vram_index].ucMemoryVenderID.ToString("X")
                });
                tableVRAM.Items.Add(new {
                    NAME = "Size (MB)",
                    VALUE = atom_vram_entries[atom_vram_index].usMemorySize
                });
                tableVRAM.Items.Add(new {
                    NAME = "Density",
                    VALUE = "0x" + atom_vram_entries[atom_vram_index].ucDensity.ToString("X")
                });
                tableVRAM.Items.Add(new {
                    NAME = "Type",
                    VALUE = "0x" + atom_vram_entries[atom_vram_index].ucMemoryType.ToString("X")
                });
            }
        }
    }
}
