using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using Syroot.Windows.IO;
using System.IO;
using Microsoft.Win32;

namespace propagacja_app
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool dane = false;
        public bool pobrane = false;
        public bool odp = false;
        List<List<int>> probki = new List<List<int>>();
        Dictionary<List<int>, List<double>> probki2 = new Dictionary<List<int>, List<double>>();
        public List<int> warstwy;
        public List<int> wyniki=new List<int>();
        public Warstwa start;
        public MainWindow()
        {
            InitializeComponent();
            finish.Text = "Jak korzystac z programu?\n 1. wczytaj strukture sieci (losowo przy pomocy min i max dla wag lub wczytaj wczesniej zapisane wagi).\n (Dodaj dodaje liczbe neutronow na warstie, zapisz konczy edycje struktury).\n 2. Wczytaj wartosci wejsciowe wyjsciowe - 1 probka w jednej linijce, wartosci oddzleone spacja (powinno ich byc tyle ile podales na pierwszej i ostatniej warstwie!).\n 3. Okresl interesujacy cie epsilon, epoki, gamme i kliknil Blad, aby nauczyc siec. \n 4. Klikniej Oblicz wartosci, aby sprawdzic jak siec sie nauczyla. \n Milej zabawy! ";
        }
        void liczWyniki(Warstwa warstwa, int skip, double beta=1)
        {          
            int w = 0;
            for (int j = 0; j < warstwy[skip+1]; j++)
            {
                double s = 0;              
                for (int i = -1; i < warstwy[skip]; w++, i++)
                {
                    if (i == -1)
                        s += 1 * warstwa.Wagi[w];
                    else
                        s += warstwa.Neutrony[i] * warstwa.Wagi[w];
                }
                double wynik = 1 / (1 + Math.Pow(Math.E, -beta * s));
                warstwa.Dzieci[0].Neutrony.Add(wynik);
            }
            if (warstwy.Count > skip + 2)
                liczWyniki(warstwa.Dzieci[0], skip + 1);
        }
        void liczWyniki2(Warstwa warstwa, int skip, double beta=1)
        {
            int w = 0;
            for (int j = 0; j < warstwy[skip+1]; j++)
            {
                double s = 0;
                for (int i = -1; i < warstwy[skip]; w++, i++)
                {
                    if (i == -1)
                        s += 1 * warstwa.Wagi[w];
                    else
                        s += warstwa.Neutrony[i] * warstwa.Wagi[w];
                }
                double wynik = 1 / (1 + Math.Pow(Math.E, -beta * s));
                warstwa.Dzieci[0].Neutrony[j]=wynik;
            }
            if (warstwy.Count > skip + 2)
                liczWyniki2(warstwa.Dzieci[0], skip + 1);     
        }
        private void otworz2Click(object sender, RoutedEventArgs e)
        {
            if (!pobrane)
            {
                MessageBox.Show("Najpierw podaj strukture!");
                return;
            }

            if (odp == true)
            {
                MessageBoxResult result = MessageBox.Show("Pobrano juz wartosci wejsciowe i wyjsciowe, zmienic?", "", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        probki2.Clear();
                        probki.Clear();
                        probki = new List<List<int>>();
                        probki2 = new Dictionary<List<int>, List<double>>();
                        break;
                    case MessageBoxResult.No:
                        break;
                }
            }
            OpenFileDialog plik;
            plik = new OpenFileDialog();
            plik.Filter = "PlikTekstowy|*.txt";
            plik.DefaultExt = ".txt";
            Nullable<bool> plikOK = plik.ShowDialog();
            if (plikOK == true)
            {
                odp = true;
                string[] linie = File.ReadAllLines(plik.FileName);
                for (int i = 0; i < linie.Length; i++)
                {
                    List<int> wejscia = new List<int>();
                    List<double> wyjscia = new List<double>();
                    var linia = linie[i].Trim();
                    var liczby = linia.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    int j = 0;
                    for (; j < warstwy[0]; j++)                    
                        wejscia.Add(int.Parse(liczby[j].Trim()));
                    
                    for (; j < warstwy[0] + warstwy[warstwy.Count - 1]; j++)
                        wyjscia.Add(double.Parse(liczby[j].Trim()));
                
                    probki2.Add(wejscia, wyjscia);
                    probki.Add(wejscia);
                }              
            }

        }
        private List<List<int>> tworzProbki()
        {
            List<List<int>> probki = new List<List<int>>();
                for (int i = 0; i < Math.Pow(2, warstwy[0]); i++) {
                probki.Add(new List<int>());
                    for (int j = 0; j < warstwy[0]; j++)
                    {
                        int p = i >> j;
                        probki[i].Add(p & 1);
                    }
            }
            return probki;
        }
        private void robWarstwy(Warstwa warstwa, List<int> warstwy, int skip)
        {
            Warstwa tmp = new Warstwa();          
                if (warstwy.Count - 1 != skip)
                   generuj(tmp, skip);
                       
            warstwa.Dzieci.Add(tmp);
            if (warstwy.Count > skip + 1)
                robWarstwy(tmp, warstwy, skip + 1);
        }
        private void generuj(Warstwa warstwa, int i)
        {
            double max;
            String tmp = inputMax.Text;
            if (!double.TryParse(tmp, out max)) { }
            double min;
            String tmp2 = inputMin.Text;
            if (!double.TryParse(tmp2, out min)) { }
            for (int j = 0; j < (warstwy[i] + 1) * warstwy[i + 1]; j++)
            { 
                Random random = new Random();
                warstwa.Wagi.Add(random.NextDouble()*(max-min)+min);
            }

        }
        private void otworz_Click(object sender, RoutedEventArgs e)
        {
            warstwy = new List<int>();
            OpenFileDialog plik;
            plik = new OpenFileDialog();
            plik.Filter = "PlikTekstowy|*.txt";
            plik.DefaultExt = ".txt";
            Nullable<bool> plikOK = plik.ShowDialog();            
            if (plikOK == true)
            {
                start = new Warstwa();
                pobrane = true;
                dane = true;
                string[] linie = File.ReadAllLines(plik.FileName);
                string zab = "5.5";
                double wynik;
                bool przecinek = double.TryParse(zab, out wynik);
                read(linie, start, 0, przecinek);         
               
            }

        }
        public void read(string[] linie, Warstwa warstwa, int poczatek, bool przecinek, string separator = " ")
        {
            Warstwa tmp = new Warstwa();
            int i = poczatek;
            for (; i < linie.Length-1; i++)
            {
                var linia = linie[i].Trim();
                if (string.IsNullOrEmpty(linia)) break;
                linia = przecinek ? linia.Replace(",", ".") : linia.Replace(".", ",");
                var liczby = linia.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);                
                for (int j = 0; j < liczby.Length; j++)
                    warstwa.Wagi.Add(double.Parse(liczby[j].Trim()));
                if (!warstwy.Any())
                    warstwy.Add(warstwa.Wagi.Count-1);
            }
            warstwa.Dzieci.Add(tmp);
            warstwy.Add(i - poczatek);
            if (poczatek == 0)
                for (int j = 1; j <= warstwy[0]; j++)
            {
                warstwa.Neutrony.Add(1);
                }
            if (i < linie.Length - 2)
                read(linie, tmp, i + 1, przecinek);
            else
            {            
                liczWyniki(start, 0);
            }
        }
        private void zapisz(Warstwa warstwa, int skip, System.IO.StreamWriter file)
        {
            int w = 0;
            string tekst = "";
            for (int j = 0; j < warstwy[skip + 1]; j++)
            {
                for (int i = 0; i < warstwy[skip] + 1; w++, i++)
                    tekst += warstwa.Wagi[w] + " ";
                file.WriteLine(tekst);
                tekst = "";
            }
            file.WriteLine("");


            if (warstwy.Count > skip + 2)
                foreach(Warstwa dziecko in warstwa.Dzieci)
                zapisz(dziecko, skip+1, file);
        }


        private void ZapiszClick(object sender, RoutedEventArgs e)
        {
            if (!dane) { 
                MessageBox.Show("Brak wag, nie mozna zapisac");
            return; }
            int w = 1;
            string path = new KnownFolder(KnownFolderType.Downloads).Path;
            path += "/dane1.txt";
            while (File.Exists(path)) { path = path.Replace("/dane"+w+".txt", "/dane" +( ++w) + ".txt");
                }
            using (System.IO.StreamWriter file =
           new System.IO.StreamWriter(path))
            {
                zapisz(start, 0, file);
            }
        }
        private void liczBlad(List<double> d, double gamma, int p)
        {
            Warstwa tmp = start;
            double sigma;
            double blad;
            int i = -1;
            List< double> dlist = new List<double>();
            for (int q = 0; q < warstwy.Count - 2-p; q++)
                tmp = tmp.Dzieci[0];
            for (int k=0 ; k < tmp.Dzieci[0].Neutrony.Count; k++)
            {
                if (p == 0)              
                    blad = (d[k]-tmp.Dzieci[0].Neutrony[k]) * gamma;               
                else
                    blad = d[k];                   
                    sigma = blad * tmp.Dzieci[0].Beta * tmp.Dzieci[0].Neutrony[k] * (1 - tmp.Dzieci[0].Neutrony[k]);
                for (int j = 1+i; j < tmp.Wagi.Count;)
                {
                    for (i = -1; i < tmp.Neutrony.Count; i++, j++)
                    {
                        if (i == -1)
                        {
                            tmp.Wagi[j] += 1 * sigma;
                            continue;
                        }
                        if (dlist.Count<i+1)
                            dlist.Add(0);
                        dlist[i]+=(tmp.Wagi[j]*sigma);
                        tmp.Wagi[j] += tmp.Neutrony[i] * sigma;
                    }
                    break;
                }            
            }
            if(tmp!=start)
            liczBlad(dlist, gamma, p + 1);
        }
        private void BladClick(object sender, RoutedEventArgs e)
        {
          
            if (!dane)
            {
                MessageBox.Show("Brak wag, nie mozna liczyc");
                return;
            }
            if (!odp)
            {
                MessageBox.Show("Podaj najpierw plik z wejsciami i wyjsciami");
                return;
            }
            String tmp2 = inputGamma.Text;
            double gamma;
            if (!double.TryParse(tmp2, out gamma)) { }
            Regex rgx = new Regex(@"^[+-]?([0-9]*[.])?[0-9]+$");
            if (rgx.IsMatch(inputGamma.Text) != true || gamma <= 0)
            {
                MessageBox.Show("Gamma- Podawaj tylko liczbe typu double i wieksza od 0!");
                inputGamma.Text = "0.1";
                return;
            }
            tmp2 = inputEpsilon.Text;
            double num;
            if (!double.TryParse(tmp2, out num)) { }
            if (rgx.IsMatch(inputEpsilon.Text) != true || num<0 || num>1)
            {
                MessageBox.Show("Epsilon- Podawaj tylko liczbe z przedzialu 0-1!");
                inputEpsilon.Text = "0";
                return;
            }
            tmp2 = inputEpoki.Text;
            int epoki;
            Regex rgx2 = new Regex(@"^\d+$");
            if (!int.TryParse(tmp2, out epoki)) { }
            if (rgx2.IsMatch(inputEpoki.Text) != true || num < 0)
            {
                MessageBox.Show("Epoki- Podawaj tylko liczbe int wieksza od 0!");
                inputEpoki.Text = "20000";
                return;
            }
            double blad=0;

            for(int i =0; i< epoki; i++)
            {
                for (int s = 0; s < probki.Count; s++)
                {
                    for (int l = 0; l < warstwy[0]; l++)
                    {
                        start.Neutrony[l] = probki[s][l];
                    }
                    List<double> dlist = new List<double>();
                    foreach (double wynik in probki2[probki[s]])
                    {
                        dlist.Add(wynik);
                    }
                    liczWyniki2(start, 0);
                    liczBlad(dlist, gamma, 0);
                    Warstwa koniec = start;
                    for (int q = 0; q < warstwy.Count - 1; q++)
                        koniec = koniec.Dzieci[0];
                    if (s == 0)
                        blad = Math.Abs(dlist[0] - koniec.Neutrony[0]);
                    for (int k = 0; k < koniec.Neutrony.Count; k++)
                        if (blad < Math.Abs(dlist[k] - koniec.Neutrony[k]))                        
                            blad = Math.Abs(dlist[k] - koniec.Neutrony[k]);                
                        
                }
                probki.Shuffle();               
                if (blad < num)                
                    break;          
                          
            }
            finish.Text = "Blad policzony kliknij Oblicz wartosci, aby sprawdzic wyniki po uczeniu.";
        }

        private void ValuesClick(object sender, RoutedEventArgs e)
        {    
            if (!dane)
            {
                MessageBox.Show("Brak wag, nie mozna obliczyc");
                return;
            }            
            finish.Text = "";
            Warstwa warstwa = start;
            for (int k = 0; k < warstwy.Count - 1; k++)
                warstwa = warstwa.Dzieci[0];       
            foreach (List<int> probka in probki)
            {
                for (int l = 0; l < warstwy[0]; l++)
                {
                    start.Neutrony[l] = probka[l];
                }          
                liczWyniki2(start, 0);
                finish.Text += "Wejscia ";
                    foreach (float neutron in start.Neutrony)
                    finish.Text += neutron + " ";
                finish.Text += ". Wynik to ";
                foreach (float neutron in warstwa.Neutrony)
                    finish.Text += neutron + " "; 
                finish.Text+="\n";
            }
        }
        private void wartosciClick(object sender, RoutedEventArgs e)
        {
            double max;
            String tmp = inputMax.Text;
            if (!double.TryParse(tmp, out max)) { }
            Regex rgx2 = new Regex(@"^[+-]?([0-9]*[.])?[0-9]+$");
            if (rgx2.IsMatch(inputMax.Text) != true)
            {
                MessageBox.Show("Max- Podawaj tylko liczbe typu double!");
                inputMax.Text = "0.2";
                return;
            }
            double min;
            String tmp3 = inputMin.Text;
            if (!double.TryParse(tmp3, out min)) { }
            Regex rgx3 = new Regex(@"^[+-]?([0-9]*[.])?[0-9]+$");
            if (rgx3.IsMatch(inputMin.Text) != true)
            {
                MessageBox.Show("Min- Podawaj tylko liczbe typu double!");
                inputMin.Text = "-0.2";
                return;
            }
            if (pobrane == true)
            {
                MessageBoxResult result = MessageBox.Show("Pobrano juz ilosc warstw, chcesz je zmienic?", "", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        warstwy.Clear();
                        pobrane = false;
                        dane = false;
                        wyniki = new List<int>();
                        break;
                    case MessageBoxResult.No:
                        break;
                }
            }
            if (pobrane == false)
            {
                InputBox.Visibility = System.Windows.Visibility.Visible;
                warstwy = new List<int>();
                napis.Text = "Podaj liczbe neuronow na tej warstwie";
            }
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {        
                String tmp2 = InputTextBox.Text;
                int num;
                if (!int.TryParse(tmp2, out num)) { }
                Regex rgx2 = new Regex(@"^\d+$");
                if (rgx2.IsMatch(InputTextBox.Text) != true || num <= 0)
                {
                    MessageBox.Show("Podawaj tylko liczbe typu int i wieksza od 0!");
                    InputTextBox.Text = String.Empty;
                    return;
                }
                warstwy.Add(num);
                InputTextBox.Text = String.Empty;
                pobrane = true;              
        }


        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            InputBox.Visibility = System.Windows.Visibility.Collapsed;
            warstwy.Clear();
            pobrane = false;
            dane = false;
            InputTextBox.Text = String.Empty;
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (warstwy.Count < 2)
            {
                MessageBox.Show("Musisz dodac przynajmniej 2 warstwy");
                return;
            }              
                dane = true;            
                List<List<int>> probki = new List<List<int>>();
                probki = tworzProbki();          
        
                start = new Warstwa();
                for (int i = 1; i <= warstwy[0]; i++)
                {
                    start.Neutrony.Add(1);
                }
                generuj(start, 0);
                robWarstwy(start, warstwy, 1);
                liczWyniki(start, 0);
                InputBox.Visibility = System.Windows.Visibility.Collapsed;               
        }


    }
    public static class ThreadSafeRandom
    {
        [ThreadStatic] private static Random Local;

        public static Random ThisThreadsRandom
        {
            get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
        }
    }
    static class MyExtensions
    {
        public static void Shuffle<T>(this IList<T> lista)
        {
            int n = lista.Count;
            while (n > 1)
            {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                T value = lista[k];
                lista[k] = lista[n];
                lista[n] = value;
            }
        }
    }
}
