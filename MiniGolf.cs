using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

/// @author Joel Hiltunen
/// @version 10.12.2019
/// <summary>
/// Kaksiulotteinen Minigolf -peli jossa tavoitteena saada pallo koloon mahdollisimman vähillä lyönneillä.
/// </summary>
public class MiniGolf : PhysicsGame
{
    private PhysicsObject kolo;
    private PhysicsObject oikeakolo;
    private PhysicsObject feikkiKolo;
    private PhysicsObject pallo;
    private readonly Image puu = LoadImage("puutekstuuri");
    private readonly Image kentta1taustakuva = LoadImage("kentta1tausta");
    private readonly Image kentta2taustakuva = LoadImage("kentta2tausta");
    private readonly Image kentta3taustakuva = LoadImage("kentta3tausta");
    private readonly Image kentta4taustakuva = LoadImage("kentta4tausta");
    private readonly Image kentta5taustakuva = LoadImage("kentta5tausta");
   
    private const int kenttienLkm = 5;
    private int kenttaNro = 1;
    private int[] parTaulukko = { 0, 2, 3, 3, 5, 3, 0, 0};
    private int[] lyonnitTaulukko = {0, 0, 0, 0, 0, 0, 0, 0};

    public override void Begin()
    {
        SeuraavaKentta();
    }


    /// <summary>
    /// vaihtaa seuraavaan kenttään, kasvattaa kenttänumeroa j lisää tarpeelliset joita kenttätiedosto ei luo
    /// </summary>
    private void SeuraavaKentta()
    {
        ClearAll();

        if (kenttaNro > kenttienLkm) kenttaNro = kenttienLkm;
        
        LuoKentta("kentta" + kenttaNro);
        LuoTulosNaytto();
        AsetaOhjaimet();
        AddCollisionHandler(pallo, oikeakolo, VauhtiPois);
        AddCollisionHandler(pallo, "vääräkolo", KoloNakyvaksi);

    }


    /// <summary>
    /// Luo uuden kentän kuvatiedostosta
    /// </summary>
    /// <param name="kenttaTiedostonNimi"> Png kuvatiedosto, nimenä kentta1, kentta2 jne. </param>
    private void LuoKentta(string kenttaTiedostonNimi)
    {
        ColorTileMap ruudut = ColorTileMap.FromLevelAsset(kenttaTiedostonNimi);

        ruudut.SetTileMethod(Color.Gold, LuoPallo);
        ruudut.SetTileMethod(Color.Black, LuoKolo);
        ruudut.SetTileMethod(Color.Red, LuoTaso);
        ruudut.SetTileMethod(Color.Cyan, LuoFeikkiKolo);
        ruudut.SetTileMethod(Color.Gray, VasenYlaKolmio);
        ruudut.SetTileMethod(Color.DarkRed, VasenAlaKolmio);
        ruudut.SetTileMethod(Color.Brown, OikeaAlaKolmio);
        ruudut.SetTileMethod(Color.Olive, OikeaYlaKolmio);

        ruudut.Optimize(Color.Red);
        ruudut.Execute(20, 20);

        Level.CreateBorders(1.0, true);
        
        switch (kenttaNro)
        {
            case 1:
                Level.Background.Image = kentta1taustakuva;
                break;

            case 2:
                Level.Background.Image = kentta2taustakuva;
                break;

            case 3:
                Level.Background.Image = kentta3taustakuva;
                break;

            case 4:
                Level.Background.Image = kentta4taustakuva;
                kolo.IsVisible = false;
                break;

            case 5:
                Level.Background.Image = kentta5taustakuva;
                break;

            default:
                Level.BackgroundColor = Color.Green;
                break;
        }
         
        Level.Background.FitToLevel();
        Camera.ZoomToLevel();
    }


    /// <summary>
    /// Luo seinän puutekstuurilla
    /// </summary>
    /// <param name="paikka">Tason sijainti</param>
    /// <param name="leveys">Tason leveys</param>
    /// <param name="korkeus">Tason korkeus</param>
    private void LuoTaso(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject taso = PhysicsObject.CreateStaticObject(leveys, korkeus);
        taso.Position = paikka;
        taso.Image = puu;
        taso.Restitution = 0.9;
        taso.CollisionIgnoreGroup = 1;
        Add(taso);
    }


    /// <summary>
    /// Luo kolon
    /// </summary>
    /// <param name="paikka">Kolon sijainti</param>
    /// <param name="leveys">Kolon leveys</param>
    /// <param name="korkeus">Kolon korkeus</param>
    private void LuoKolo(Vector paikka, double leveys, double korkeus)
    {
        kolo = new PhysicsObject(40, 40);
        kolo.Shape = Shape.Circle;
        kolo.Position = paikka;
        kolo.Color = Color.Black;
        kolo.IgnoresCollisionResponse = true;

        oikeakolo = new PhysicsObject(7, 7);
        oikeakolo.Shape = Shape.Circle;
        oikeakolo.IsVisible = false;
        oikeakolo.Position = paikka;
        oikeakolo.IgnoresCollisionResponse = true;

        Add(kolo, -1);
        Add(oikeakolo, -1);
    }


    /// <summary>
    /// Tekee kolon näköisen olion, johon osuttaessa oikea kolo tuleee näkyviin
    /// </summary>
    /// <param name="paikka">Feikkikolon sijainti</param>
    /// <param name="leveys">Feikkikolon leveys</param>
    /// <param name="korkeus">Feikkikolon korkeus</param>
    private void LuoFeikkiKolo(Vector paikka, double leveys, double korkeus)
    {
        feikkiKolo = new PhysicsObject(45, 45);
        feikkiKolo.Shape = Shape.Circle;
        feikkiKolo.Position = paikka;
        feikkiKolo.Color = Color.Black;
        feikkiKolo.LinearDamping = 0.97;
        feikkiKolo.Tag = "vääräkolo";
        Add(feikkiKolo, -1);
    }


    /// <summary>
    /// Tekee oikan kolon näkyväksi kun pallo osuu väärään koloon
    /// </summary>
    /// <param name="pallo">Törmääjä</param>
    /// <param name="feikkikolo">Törmäyksen kohde</param>
    private void KoloNakyvaksi(PhysicsObject pallo, PhysicsObject feikkikolo)
    {
        kolo.IsVisible = true;
    }


    /// <summary>
    /// Luo golfpallon
    /// </summary>
    /// <param name="paikka">Pallon sijainti</param>
    /// <param name="leveys">Pallon leveys</param>
    /// <param name="korkeus">Pallon korkeus</param>
    private void LuoPallo(Vector paikka, double leveys, double korkeus)
    {
        pallo = new PhysicsObject(15.0, 15.0);
        pallo.Shape = Shape.Circle;
        pallo.Position = paikka;
        pallo.Restitution = 1.0;
        pallo.LinearDamping = 0.99;
        pallo.MaxVelocity = 800;
        pallo.Tag = "pallo";
        Add(pallo);
    }


    /// <summary>
    ///  Luo harmaan kolmion jonka suora kulma osoittaa vasempaan yläviistoon
    /// </summary>
    /// <param name="paikka">Komion sijainti</param>
    /// <param name="leveys">Kolmion leveys</param>
    /// <param name="korkeus">Kolmion korkeus</param>
    private void VasenYlaKolmio(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject kolmio = new PhysicsObject(120, 60);
        kolmio.Color = Color.Gray;
        kolmio.Shape = Shape.Triangle;
        kolmio.Angle = Angle.FromDegrees(45);
        kolmio.Mass = 100000;
        kolmio.Position = paikka + new Vector(-20, 20);
        kolmio.CanRotate = false;
        Add(kolmio);
    }


    /// <summary>
    /// Luo harmaan kolmion jonka suora kulma osoittaa vasempaan alaviistoon
    /// </summary>
    /// <param name="paikka">Kolmion sijainti</param>
    /// <param name="leveys">Kolmion leveys</param>
    /// <param name="korkeus">Kolmion korkeus</param>
    private void VasenAlaKolmio(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject kolmio = new PhysicsObject(120, 60);
        kolmio.Color = Color.Gray;
        kolmio.Shape = Shape.Triangle;
        kolmio.Angle = Angle.FromDegrees(135);
        kolmio.Mass = 100000;
        kolmio.Position = paikka + new Vector(-20, -20);
        kolmio.CanRotate = false;
        Add(kolmio);
    }


    /// <summary>
    /// Luo harmaan kolmion jonka suora kulma osoittaa oikeaan alaviistoon
    /// </summary>
    /// <param name="paikka">Kolmion sijainti</param>
    /// <param name="leveys">Kolmion leveys</param>
    /// <param name="korkeus">Kolmion korkeus</param>
    private void OikeaAlaKolmio(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject kolmio = new PhysicsObject(120, 60);
        kolmio.Color = Color.Gray;
        kolmio.Shape = Shape.Triangle;
        kolmio.Angle = Angle.FromDegrees(-135);
        kolmio.Mass = 100000;
        kolmio.Position = paikka + new Vector(20, -20);
        kolmio.CanRotate = false;
        Add(kolmio);
    }


    /// <summary>
    /// Luo harmaan kolmion jonka suora kulma osoittaa oikeaan yläviistoon
    /// </summary>
    /// <param name="paikka">Kolmion sijainti</param>
    /// <param name="leveys">Kolmion leveys</param>
    /// <param name="korkeus">Kolmion korkeus</param>
    private void OikeaYlaKolmio(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject kolmio = new PhysicsObject(120, 60);
        kolmio.Color = Color.Gray;
        kolmio.Shape = Shape.Triangle;
        kolmio.Angle = Angle.FromDegrees(-45);
        kolmio.Mass = 100000;
        kolmio.Position = paikka + new Vector(20, 20);
        kolmio.CanRotate = false;
        Add(kolmio);
    }


    /// <summary>
    /// Näytöt jota näyttää par lukeman sekä tämänhetkisen kierroksen lyönnit
    /// </summary>
    private void LuoTulosNaytto()
    {
        double x = Screen.Left + 400;
        double y = Screen.Top - 50;

        Label tuloksesi = new Label("Tuloksesi");

        tuloksesi.X = x - 50;
        tuloksesi.Y = y - 15;
        Add(tuloksesi);

        Label parLabel = new Label("Par");
       
        parLabel.X = x - 50;
        parLabel.Y = y + 15;
        Add(parLabel);

        for (int i = 1; i < kenttienLkm + 1; i++)
        {
            Label tulosJaLyonnit = new Label("" + lyonnitTaulukko[i]);
            tulosJaLyonnit.X = x + 20;
            tulosJaLyonnit.Y = y - 15;
            tulosJaLyonnit.TextColor = Color.Black;
            tulosJaLyonnit.Color = Color.Gold;
            Add(tulosJaLyonnit);
            x += 25;
        }

        x = Screen.Left + 400;
        y = Screen.Top - 50;

        for (int i = 1; i < kenttienLkm + 1; i++)
        {
            Label tulosJaLyonnit = new Label("" + parTaulukko[i]);
            tulosJaLyonnit.X = x + 20;
            tulosJaLyonnit.Y = y + 15;
            tulosJaLyonnit.TextColor = Color.Black;
            tulosJaLyonnit.Color = Color.Gold;
            Add(tulosJaLyonnit);
            x += 25;
        }
    }
    
  
    /// <summary>
    /// Tekee apuviivan lyöntiä varten kun pallo on tarpeeksi paikallaan
    /// </summary>
    protected override void Paint(Canvas canvas)
    {
        double pituus = (pallo.Position - Mouse.PositionOnWorld).Magnitude;
        if (pallo.Velocity.Magnitude < 5 && pituus < 300)
        {
            canvas.BrushColor = Color.White;

            Vector keskipiste = (pallo.Position);
            Vector reunapiste = pallo.Position - Mouse.PositionOnWorld;

            canvas.DrawLine(keskipiste, keskipiste + reunapiste);

            base.Paint(canvas);
        }
    }


    /// <summary>
    /// Aliohjelma lyö palloa poispäin hiirestä kun sitä klikataan.
    /// Voima määräytyy hiiren etäisyydestä pallosta.
    /// Lisää myös lyönnin laskuriin
    /// </summary>
    private void Lyonti()
    {
        Vector suunta = (pallo.AbsolutePosition - Mouse.PositionOnWorld);
        if(suunta.Magnitude > 300) return;
        if (pallo.Velocity.Magnitude < 5)
        {
            pallo.Hit(suunta * 4);
            lyonnitTaulukko[kenttaNro] += 1;
            LuoTulosNaytto();
        }
    }


    /// <summary>
    /// Pysäyttää pallon kun se osuu kolon keskelle. 
    /// </summary>
    private void VauhtiPois(PhysicsObject pallo, PhysicsObject kolo)
    {
        if (pallo.Velocity.Magnitude < 550)
        {
            pallo.Velocity = pallo.Velocity * 0;
            Koloon();
        }
    }


    /// <summary>
    /// VAihtaa seuraavaan kenttään kun pallo osuu koloon ja näyttää viestin. Lisää alussa luotuun lyönnit -taulukkoon tämänhetkiset lyönnit
    /// </summary>
    private void Koloon()
    {
        if (lyonnitTaulukko[kenttaNro] == 1) MessageDisplay.Add("Hole In One!");
        if (parTaulukko[kenttaNro] - lyonnitTaulukko[kenttaNro] == 2) MessageDisplay.Add("Eagle");
        if (parTaulukko[kenttaNro] - lyonnitTaulukko[kenttaNro] == 1) MessageDisplay.Add("Birdie");
        if (parTaulukko[kenttaNro] - lyonnitTaulukko[kenttaNro] == 0) MessageDisplay.Add("Par");
        if (parTaulukko[kenttaNro] - lyonnitTaulukko[kenttaNro] == -1) MessageDisplay.Add("Bogey");
        if (parTaulukko[kenttaNro] - lyonnitTaulukko[kenttaNro] == -2) MessageDisplay.Add("Double Bogey");
        if (parTaulukko[kenttaNro] - lyonnitTaulukko[kenttaNro] == -3) MessageDisplay.Add("Triple Bogey");
        if (parTaulukko[kenttaNro] - lyonnitTaulukko[kenttaNro] == -4) MessageDisplay.Add("....");
        if (parTaulukko[kenttaNro] - lyonnitTaulukko[kenttaNro] == -5) MessageDisplay.Add("....");

        MessageDisplay.TextColor = Color.Red;

        if (kenttaNro == kenttienLkm) LopetusNakyma();
        else
        {
            kenttaNro++;
            Timer.SingleShot(1.5, SeuraavaKentta);
        } 
    }


    /// <summary>
    /// Luo lopetusnäkymän jossa näkyy tulos
    /// </summary>
    private void LopetusNakyma()
    {
        int lopputulos = 0;
        int parYhteen = 0;
        for (int i = 1; i < kenttaNro + 1; i++)
        {
             lopputulos += lyonnitTaulukko[i];
        }

        for (int i = 0; i < kenttaNro + 1; i++)
        {
            parYhteen +=  parTaulukko[i];
        }

        lopputulos = lopputulos - parYhteen;

        MultiSelectWindow valikko = new MultiSelectWindow("Tuloksesi on " + lopputulos,
        "Uusi peli", "Lopeta");
        valikko.ItemSelected += PainettiinValikonNappia;
        Add(valikko);

    }


    /// <summary>
    /// Aloittaa pelin alusta tai poistuu pelistä
    /// </summary>
    /// <param name="valinta">kumpaa nappulaa on klikattu</param>
    private void PainettiinValikonNappia(int valinta)
    {
        switch (valinta)
        {
            case 0:
                for(int i = 1; i < kenttienLkm + 1; i++)
                lyonnitTaulukko[i] = 0;
                kenttaNro = 1;
                SeuraavaKentta();
                break;

            case 1:
                Exit();
                break;
        }
    }


    /// <summary>
    /// Asettaa peliin ohjaimet
    /// </summary>
    private void AsetaOhjaimet()
    {
        Mouse.IsCursorVisible = true;
        Mouse.Listen(MouseButton.Left, ButtonState.Released, Lyonti, "Lyö palloa hiirellä klikkaamalla");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.R, ButtonState.Pressed, SeuraavaKentta, "Aloita kenttä uudestaan");

    }
}