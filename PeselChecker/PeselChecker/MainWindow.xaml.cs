using System;
using System.Windows;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Windows.Controls;
using System.Xml.Serialization;
using Microsoft.Win32;
using PeselChecker.Classes;

namespace PeselChecker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        //private readonly ArrayList _peselList = new ArrayList();
        private List<object> _peselList = new List<object>();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonCheck_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                String tempPesel = PeselTextBoxPesel.Text;//pobieram pesel z tb
                bool genderSelect = (bool)PeselRadioButtonMen.IsChecked;//pobieram płeć z radio buttonow
                DateTime dateSelect = new DateTime(PeselDatePickerBirthDate.SelectedDate.Value.Year, PeselDatePickerBirthDate.SelectedDate.Value.Month, PeselDatePickerBirthDate.SelectedDate.Value.Day);//pobieram date z date pickera
                Pesel.CheckPesel(tempPesel);
                Pesel.CheckGender(genderSelect, tempPesel);
                Pesel.CheckDateOfBirth(dateSelect, tempPesel);
                MessageBox.Show("Wprowadzone dane są poprawne");
                _peselList.Add(tempPesel);//dodaje do listy pesel
                RefreshListBox(PeselNumberListBoxPesel, _peselList);
            }
            catch (PeselNumberException exc)
            {
                MessageBox.Show(exc.Message);
            }
            catch (NullReferenceException exc)
            {
                MessageBox.Show(exc.Message);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void PeselTextBoxPesel_GotFocus(object sender, RoutedEventArgs e)
        {
            PeselTextBoxPesel.Text = String.Empty;
        }

        private void PeselTextBoxPesel_LostFocus(object sender, RoutedEventArgs e)
        {
            if (PeselTextBoxPesel.Text == String.Empty)
            {
                PeselTextBoxPesel.Text = "Podaj Pesel";
            }
        }

        private void ButtonLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    FileName = "PeselNumbers",
                    Filter = "(*.bin)|*.bin|(*.xml)|*.xml|(*.json)|*.json|All files (*.*)|*.*",
                    RestoreDirectory = true,
                    Title = "Podaj nazwę pliku"
                };
                short selectedTypeOfSerializable = 0;
                if (SerializableButtonBin.IsChecked == true)
                    selectedTypeOfSerializable = 0;
                else if (SerializableButtonXml.IsChecked == true)
                    selectedTypeOfSerializable = 1;
                else if (SerializableButtonJson.IsChecked == true)
                    selectedTypeOfSerializable = 2;
                switch (selectedTypeOfSerializable)
                {
                    case 0://bin
                        openFileDialog.DefaultExt = "bin";
                        openFileDialog.Filter = "(*.bin)|*.bin|(*.gz)|*.gz|All files (*.*)|*.*";
                        break;
                    case 1://xml
                        openFileDialog.DefaultExt = "xml";
                        openFileDialog.Filter = "(*.xml)|*.xml|(*.gz)|*.gz|All files (*.*)|*.*";
                        break;
                    case 2://json
                        openFileDialog.DefaultExt = "json";
                        openFileDialog.Filter = "(*.json)|*.json|(*.gz)|*.gz|All files (*.*)|*.*";
                        break;
                    default:
                        openFileDialog.DefaultExt = "bin";
                        openFileDialog.Filter = "(*.bin)|*.bin|(*.xml)|*.xml|(*.json)|*.json|(*.gz)|*.gz|All files (*.*)|*.*";
                        break;
                }
                if (openFileDialog.ShowDialog() == true)
                {
                    if ((bool)(SerializableButtonGZip.IsChecked = true))
                    {
                        FileInfo fileToDecompress = new FileInfo(openFileDialog.FileName);
                        using (FileStream originalFileStream = fileToDecompress.OpenRead())
                        {
                            string currentFileName = fileToDecompress.FullName;
                            string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

                            using (FileStream decompressedFileStream = File.Create(newFileName))
                            {
                                using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                                {
                                    decompressionStream.CopyTo(decompressedFileStream);
                                }
                            }
                        }
                    }
                    using (Stream buffer = openFileDialog.OpenFile())
                    {
                        switch (selectedTypeOfSerializable)
                        {
                            case 0://bin
                                IFormatter binaryFormatter = new BinaryFormatter();
                                _peselList = binaryFormatter.Deserialize(buffer) as List<object>;
                                break;
                            case 1://xml
                                XmlSerializer xmlSerializer = new XmlSerializer(_peselList.GetType());
                                _peselList = xmlSerializer.Deserialize(buffer) as List<object>;
                                break;
                            case 2://json
                                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(_peselList.GetType());
                                _peselList = jsonSerializer.ReadObject(buffer) as List<object>;
                                break;
                            default://fail!!!!!
                                throw new Exception("Nieoczekiwana akcja!");
                        }
                        RefreshListBox(PeselNumberListBoxPesel, _peselList);
                    }
                }
            }
            catch (SerializationException exc)
            {
                MessageBox.Show(exc.Message);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    FileName = "PeselNumbers",
                    Filter = "(*.bin)|*.bin|(*.xml)|*.xml|(*.json)|*.json|All files (*.*)|*.*",
                    Title = "Podaj nazwę pliku",
                };
                short selectedTypeOfSerializable = 0;
                if (SerializableButtonBin.IsChecked == true)
                    selectedTypeOfSerializable = 0;
                else if (SerializableButtonXml.IsChecked == true)
                    selectedTypeOfSerializable = 1;
                else if (SerializableButtonJson.IsChecked == true)
                    selectedTypeOfSerializable = 2;
                switch (selectedTypeOfSerializable)
                {
                    case 0://bin
                        saveFileDialog.DefaultExt = "bin";
                        saveFileDialog.Filter = "(*.bin)|*.bin|All files (*.*)|*.*";
                        break;
                    case 1://xml
                        saveFileDialog.DefaultExt = "xml";
                        saveFileDialog.Filter = "(*.xml)|*.xml|All files (*.*)|*.*";
                        break;
                    case 2://json
                        saveFileDialog.DefaultExt = "json";
                        saveFileDialog.Filter = "(*.json)|*.json|All files (*.*)|*.*";
                        break;
                    default:
                        saveFileDialog.DefaultExt = "bin";
                        saveFileDialog.Filter = "(*.bin)|*.bin|(*.xml)|*.xml|(*.json)|*.json|All files (*.*)|*.*";
                        break;
                }
                if (saveFileDialog.ShowDialog() == true)
                {
                    FileStream buffer = File.Create(Path.Combine(Path.GetDirectoryName(saveFileDialog.FileName), saveFileDialog.FileName));
                    switch (selectedTypeOfSerializable)
                    {
                        case 0://bin
                            IFormatter binaryFormatter = new BinaryFormatter();
                            binaryFormatter.Serialize(buffer, _peselList);
                            break;
                        case 1://xml
                            XmlSerializer xmlSerializer = new XmlSerializer(_peselList.GetType());
                            xmlSerializer.Serialize(buffer, _peselList);
                            break;
                        case 2://json
                            DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(_peselList.GetType());
                            jsonSerializer.WriteObject(buffer, _peselList);
                            break;
                        default://fail!!!!!
                            throw new Exception("Nieoczekiwana akcja!");
                    }
                    buffer.Close();
                    if ((bool)(SerializableButtonGZip.IsChecked = true))
                    {
                        FileInfo fileToCompress = new FileInfo(saveFileDialog.FileName);
                        MessageBox.Show(fileToCompress.FullName);
                        using (FileStream originalFileStream = fileToCompress.OpenRead())
                        {
                            if ((File.GetAttributes(fileToCompress.FullName) & FileAttributes.Hidden) != FileAttributes.Hidden & fileToCompress.Extension != ".gz")
                            {
                                using (FileStream compressedFileStream = File.Create(Path.Combine(Path.GetDirectoryName(saveFileDialog.FileName), Path.GetFileNameWithoutExtension(saveFileDialog.FileName)) + ".gz"))
                                {
                                    using (GZipStream compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
                                    {
                                        originalFileStream.CopyTo(compressionStream);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (SerializationException exc)
            {
                MessageBox.Show(exc.Message);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void RefreshListBox(ListBox myListBox, List<object> myList)
        {
            PeselNumberListBoxPesel.Items.Clear();//czyszcze listbox
            _peselList.Sort();//sortowanie listy
            foreach (var pn in _peselList)
            {
                PeselNumberListBoxPesel.Items.Add(pn);//dodaje od poczatku do konca pesel do lb
            }
        }
    }
}