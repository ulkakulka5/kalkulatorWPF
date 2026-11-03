using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private double pamiec = 0;
        private bool czyNoweDzialanie = false;

        public MainWindow()
        {
            InitializeComponent();
        }

       
        private string PobierzEkran()
        {
            return poleEkran.Content?.ToString() ?? "0";
        }

        private void PrzyciskZnak_Click(object sender, RoutedEventArgs e)
        {
            Button przycisk = (Button)sender;
            string? znak = przycisk.Content.ToString();

            if (czyNoweDzialanie)
            {
                if ("+-*/^".Contains(znak))
                {
                    poleEkran.Content = PobierzEkran() + znak;
                    etykietaHistoria.Content = PobierzEkran();
                    czyNoweDzialanie = false;
                    return;
                }

                poleEkran.Content = "0";
                etykietaHistoria.Content = "";
                czyNoweDzialanie = false;
            }

            string tekst = PobierzEkran();

            if (tekst == "0" && char.IsDigit(znak[0]))
            {
                poleEkran.Content = znak;
            }
            else
            {
                poleEkran.Content = tekst + znak;
            }
        }

        private void PrzyciskPrzecinek_Click(object sender, RoutedEventArgs e)
        {
            if (czyNoweDzialanie)
            {
                poleEkran.Content = "0,";
                etykietaHistoria.Content = "";
                czyNoweDzialanie = false;
                return;
            }

            string tekst = PobierzEkran();

            int indeks = tekst.Length - 1;

            while (indeks >= 0 &&
                   (char.IsDigit(tekst[indeks]) || tekst[indeks] == ','))
            {
                indeks--;
            }

            string liczba = tekst.Substring(indeks + 1);

            if (!liczba.Contains(","))
            {
                poleEkran.Content = tekst +
                    (liczba.Length == 0 ? "0," : ",");
            }
        }

        private void PrzyciskPierwiastek_Click(object sender, RoutedEventArgs e)
        {
            if (czyNoweDzialanie || PobierzEkran() == "0")
            {
                poleEkran.Content = "√(";
                etykietaHistoria.Content = "";
                czyNoweDzialanie = false;
            }
            else
            {
                poleEkran.Content = PobierzEkran() + "√(";
            }
        }

        private void PrzyciskUlamek_Click(object sender, RoutedEventArgs e)
        {
            if (czyNoweDzialanie || PobierzEkran() == "0")
            {
                poleEkran.Content = "1/(";
                etykietaHistoria.Content = "";
                czyNoweDzialanie = false;
            }
            else
            {
                poleEkran.Content = PobierzEkran() + "1/(";
            }
        }

        private void PrzyciskZmienZnak_Click(object sender, RoutedEventArgs e)
        {
            string tekst = PobierzEkran();

            if (tekst == "0")
                return;

            if (czyNoweDzialanie)
            {
                czyNoweDzialanie = false;
                etykietaHistoria.Content = "";
            }

            poleEkran.Content = tekst.StartsWith("-")
                ? tekst.Substring(1)
                : "-" + tekst;
        }

        private void PrzyciskCofnij_Click(object sender, RoutedEventArgs e)
        {
            if (czyNoweDzialanie)
            {
                PrzyciskWyczysc_Click(sender, e);
                return;
            }

            string tekst = PobierzEkran();

            poleEkran.Content = tekst.Length > 1
                ? tekst.Substring(0, tekst.Length - 1)
                : "0";
        }

        private void PrzyciskWyczysc_Click(object sender, RoutedEventArgs e)
        {
            poleEkran.Content = "0";
            etykietaHistoria.Content = "";
            czyNoweDzialanie = false;
        }

        private void PrzyciskMS_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                pamiec = ObliczWyrazenie(PobierzEkran());

                przyciskMR.Content = "MR*";

                MessageBox.Show(
                    "Zapisano do pamięci: " + pamiec,
                    "Pamięć");
            }
            catch (Exception blad)
            {
                MessageBox.Show(
                    "Nie zapisano wartości: " + blad.Message,
                    "Błąd");
            }
        }

        private void PrzyciskMR_Click(object sender, RoutedEventArgs e)
        {
            string liczba = pamiec.ToString(CultureInfo.CurrentCulture);

            poleEkran.Content = czyNoweDzialanie || PobierzEkran() == "0"
                ? liczba
                : PobierzEkran() + liczba;

            etykietaHistoria.Content = "";
            czyNoweDzialanie = false;
        }

        private void PrzyciskMC_Click(object sender, RoutedEventArgs e)
        {
            pamiec = 0;
            przyciskMR.Content = "MR";

            MessageBox.Show(
                "Pamięć została wyczyszczona.",
                "Pamięć");
        }

        private void PrzyciskRownaSie_Click(object sender, RoutedEventArgs e)
        {
            string wyrazenie = PobierzEkran();

            try
            {
                double wynik = ObliczWyrazenie(wyrazenie);

                if (double.IsNaN(wynik) || double.IsInfinity(wynik))
                    throw new ArithmeticException("Wynik jest poza zakresem.");

                etykietaHistoria.Content = wyrazenie + " =";

                poleEkran.Content = wynik.ToString(
                    "G15",
                    CultureInfo.CurrentCulture);

                czyNoweDzialanie = true;
            }
            catch (DivideByZeroException)
            {
                MessageBox.Show(
                    "Nie można dzielić przez zero!",
                    "Błąd matematyczny");
            }
            catch (Exception blad)
            {
                MessageBox.Show(
                    "Sprawdź wyrażenie: " + blad.Message,
                    "Błąd składni");
            }
        }

        private double ObliczWyrazenie(string wyrazenie)
        {
            wyrazenie = wyrazenie.Replace(" ", "")
                                 .Replace(",", ".");

            int indeks = 0;

            double ParsujDodawanieOdejmowanie()
            {
                double wartosc = ParsujMnozenieDzielenie();

                while (indeks < wyrazenie.Length &&
                      (wyrazenie[indeks] == '+' ||
                       wyrazenie[indeks] == '-'))
                {
                    char dzialanie = wyrazenie[indeks++];

                    double kolejna = ParsujMnozenieDzielenie();

                    wartosc = dzialanie == '+'
                        ? wartosc + kolejna
                        : wartosc - kolejna;
                }

                return wartosc;
            }

            double ParsujMnozenieDzielenie()
            {
                double wartosc = ParsujPotege();

                while (indeks < wyrazenie.Length &&
                      (wyrazenie[indeks] == '*' ||
                       wyrazenie[indeks] == '/'))
                {
                    char dzialanie = wyrazenie[indeks++];

                    double kolejna = ParsujPotege();

                    if (dzialanie == '/' && kolejna == 0)
                        throw new DivideByZeroException();

                    wartosc = dzialanie == '*'
                        ? wartosc * kolejna
                        : wartosc / kolejna;
                }

                return wartosc;
            }

            double ParsujPotege()
            {
                double podstawa = ParsujCzynnik();

                if (indeks < wyrazenie.Length &&
                    wyrazenie[indeks] == '^')
                {
                    indeks++;

                    double wykladnik = ParsujPotege();

                    return Math.Pow(podstawa, wykladnik);
                }

                return podstawa;
            }

            double ParsujCzynnik()
            {
                if (indeks >= wyrazenie.Length)
                    throw new FormatException(
                        "Brakuje liczby lub nawiasu.");

                if (wyrazenie[indeks] == '-' ||
                    wyrazenie[indeks] == '+')
                {
                    char znak = wyrazenie[indeks++];

                    double wartosc = ParsujCzynnik();

                    return znak == '-' ? -wartosc : wartosc;
                }

                if (wyrazenie[indeks] == '(')
                {
                    indeks++;

                    double wartosc = ParsujDodawanieOdejmowanie();

                    if (indeks >= wyrazenie.Length ||
                        wyrazenie[indeks] != ')')
                    {
                        throw new FormatException(
                            "Brakuje nawiasu zamykającego ).");
                    }

                    indeks++;

                    return wartosc;
                }

                 
                if (wyrazenie[indeks] == '√')
                {
                    indeks++;

                    double liczba = ParsujCzynnik();

                    if (liczba < 0)
                        throw new ArithmeticException(
                            "Nie można obliczyć pierwiastka z liczby ujemnej.");

                    return Math.Sqrt(liczba);
                }

                int poczatek = indeks;

                while (indeks < wyrazenie.Length &&
                      (char.IsDigit(wyrazenie[indeks]) ||
                       wyrazenie[indeks] == '.'))
                {
                    indeks++;
                }

                if (poczatek == indeks)
                {
                    throw new FormatException(
                        "Niepoprawny znak: " + wyrazenie[indeks]);
                }

                string liczbaTekst = wyrazenie.Substring(
                    poczatek,
                    indeks - poczatek);

                return double.Parse(
                    liczbaTekst,
                    CultureInfo.InvariantCulture);
            }

            if (string.IsNullOrWhiteSpace(wyrazenie))
                throw new FormatException("Puste wyrażenie.");

            double wynik = ParsujDodawanieOdejmowanie();

            if (indeks != wyrazenie.Length)
            {
                throw new FormatException(
                    "Nieoczekiwany znak: " + wyrazenie[indeks]);
            }

            return wynik;
        }

        private void PrzyciskC_Click(object sender, RoutedEventArgs e)
        {
            string tekst = PobierzEkran();
            

        }

        private void PrzyciskF_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
