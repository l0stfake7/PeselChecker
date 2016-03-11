using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using PeselChecker.Interfaces;

namespace PeselChecker.Classes
{
    [Serializable]
    public sealed class Pesel : IIdNumber, IComparable<Pesel>, IComparer<Pesel>, ISerializable
    {
        [NonSerialized]
        private const short PeselLength = 11;

        public int Gender { get; set; }

        public DateTime DateOfBirth { get; set; }

        private object _peselNumber;

        public object PeselNumber
        {
            get { return _peselNumber; }
            set
            {
                switch (Type.GetTypeCode(value.GetType()))
                {
                    case TypeCode.String:
                        if (CheckPesel(value.ToString()))
                        {
                            _peselNumber = value.ToString();
                        }
                        break;
                    case TypeCode.Int64: //long
                        if (CheckPesel(Convert.ToInt64(value)))
                        {
                            _peselNumber = Convert.ToInt64(value);
                        }
                        break;
                    case TypeCode.Char:
                        if (CheckPesel(Convert.ToChar(value)))
                        {
                            _peselNumber = Convert.ToChar(value);
                        }
                        break;
                    default:
                        _peselNumber = "";
                        throw new PeselNumberException("Nieobsługiwany typ danych!");
                }
            }
        }

        public Pesel()
        {
            PeselNumber = String.Empty;
        }

        public Pesel(String pesel)
        {
            PeselNumber = pesel;
        }

        public Pesel(long pesel)
        {
            PeselNumber = pesel.ToString();
        }

        public Pesel(char[] pesel)
        {
            String tempString = new String(pesel);
            PeselNumber = tempString;
        }

        public Pesel(SerializationInfo info, StreamingContext context)//deserializacja - odczyt
        {
            Gender = (int)info.GetValue("Gender", typeof(int));
            DateOfBirth = (DateTime)info.GetValue("DateOfBirth", typeof(DateTime));
            PeselNumber = info.GetValue("PeselNumber", typeof(object));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)//serializacja - zapis
        {
            info.AddValue("Gender", Gender);
            info.AddValue("DateOfBirth", DateOfBirth);
            info.AddValue("PeselNumber", PeselNumber);
        }

        public int CompareTo(Pesel psl)
        {
            return Convert.ToInt32(PeselNumber).CompareTo(Convert.ToInt32(psl.PeselNumber));
        }

        public int Compare(Pesel peselNumberFirst, Pesel peselNumberSecond)
        {
            if (Convert.ToInt64(peselNumberFirst.PeselNumber) > Convert.ToInt64(peselNumberSecond.PeselNumber))
                return 1;
            else if (Convert.ToInt64(peselNumberFirst.PeselNumber) < Convert.ToInt64(peselNumberSecond.PeselNumber))
                return -1;
            else
                return 0;
        }

        private static bool GetGender(String pesel)
        {
            int gender = (int.Parse(pesel[pesel.Length - 2 /*11-2=9 indeks*/].ToString()) % 2);
            return (gender == 1);
            //jeśli reszta z dzielenia to jeden, oznacza to, że mamy mężczyznę, w przeciwnym razie mamy kobietę          
        }

        public static bool CheckGender(bool gender, String pesel)
        {
            if (gender != GetGender(pesel))
            {
                throw new PeselNumberException("Wybrana płeć jest niezgodna z podanym numerem Pesel");
            }
            return true;
        }

        private static DateTime GetDateOfBirth(String pesel)
        {
            int day,
                month,
                year;
            string tempString;
            if (int.Parse(pesel.Substring(2, 2)) <= 12) //1900 – 1999
            {
                tempString = string.Format("{0}{1}", "19", int.Parse(pesel.Substring(0, 2)));
                month = int.Parse(pesel.Substring(2, 2));

            }
            else //2000 – 2099 - odjac od miesiaca 20
            {
                if (int.Parse(pesel.Substring(0, 2)) < 10)
                {
                    tempString = string.Format("{0}{1}{2}", "20", "0", int.Parse(pesel.Substring(0, 2)));
                }
                else
                {
                    tempString = string.Format("{0}{1}", "20", int.Parse(pesel.Substring(0, 2)));
                }
                month = int.Parse(pesel.Substring(2, 2)) - 20;
            }
            year = int.Parse(tempString);
            day = int.Parse(pesel.Substring(4, 2));
            DateTime dateOfBirth = new DateTime(year, month, day);
            return dateOfBirth;
        }

        public static bool CheckDateOfBirth(DateTime date, String pesel)
        {
            if (date.Date.Equals(GetDateOfBirth(pesel).Date) == false)
            {
                throw new PeselNumberException("Wybrana data jest niezgodna z podanym numerem Pesel");
            }
            return true;
        }

        public static int CalculateControlSum(String input, int[] weights)
        {
            int controlSum = 0;
            for (int i = 0; i < input.Length - 1; i++)
            {
                controlSum += weights[i] * int.Parse(input[i].ToString());
                //Convert.ToInt32(input[i].ToString());//zmieniam znak asci odpowiedniego znaku pesel-ciągu na cyfrę
            }
            return controlSum;
        }

        public static bool CheckPesel(String pesel)
        {
            int[] scales = //wagi dla każdej kolejnej cyfry numeru pesel
            {
                1, 3, 7, 9, 1, 3, 7, 9, 1, 3
            };
            Regex r = new Regex("^[0-9]*$");
            if (pesel == null)
            {
                throw new NullReferenceException("Wykryto odwołanie do obiektu o wartosc null");
            }
            if (pesel.Length == 0)
            {
                throw new PeselNumberException("Nic nie wpisałeś!");
            }
            if (pesel.Length != PeselLength)
            {
                throw new PeselNumberException("Nieprawidłowa długość numeru PESEL!");
            }
            if (!r.IsMatch(pesel))
            {
                throw new PeselNumberException("Wykryto nieprawdiłowe znaki, PESEL może zawierać tylko cyfry!");
            }
            var controlSum = CalculateControlSum(pesel, scales);
            int controlNum = controlSum % 10;
            controlNum = 10 - controlNum;
            if (controlNum == 10)
            {
                controlNum = 0;
            }
            int lastDigit = int.Parse(pesel[pesel.Length - 1].ToString());
            if (controlNum != lastDigit)
            {
                throw new PeselNumberException("Nieprawidłowa suma kontrolna, taki PESEL nie istnieje!");
            }
            return true;
        }

        public static bool CheckPesel(long pesel)
        {
            return CheckPesel(pesel.ToString());
        }

        public static bool CheckPesel(char[] pesel)
        {
            String tempString = new String(pesel);
            return CheckPesel(tempString);
        }
    }
}
/*  5. Czym się różni interfejs IComparable od IComparer?
    IComparable wymusza implementacje metody CompareTo(), która porównuje obiekt przekazany(inny) obiekt z aktualnym o tych samych typach
    IComparer wymusza implementacje metody Compare(), które porównuje dwa obiekty o dowolnych typach, przydatne, gdy nie mozemy skorzystac z IComparable
*/
