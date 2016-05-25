using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Gabriel.Cat;
using Microsoft.Win32;
using Gabriel.Cat.Extension;
using System.ComponentModel;
using System.Drawing;

namespace Pokedex
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FrameWorkPokemonGBA.RomPokemon rom;
        PokemonPokedex pokemonActual;
        System.Drawing.Color colorSelected;
        bool hayCambios;
        private bool hayCambiosPokemonActual;
        static readonly char[] caracteresNoNumericos=CaracteresNoNumericos();
        bool huevoActivado;
        bool shinyActivado;
        PokemonPokedex[] pokedexCargada;
        public MainWindow()
        {
            ContextMenu menuContextual;
            MenuItem opcionMenu;
            huevoActivado = false;
            hayCambios = false;
            InitializeComponent();
            Closed += GuardaRom;
            pltNormal.ColorPicker.IsAlfaEnabled = false;
            pltShiny.ColorPicker.IsAlfaEnabled = false;
            hayCambiosPokemonActual = false;
            menuContextual = new ContextMenu();
            opcionMenu = new MenuItem();
            opcionMenu.Header = "Cargar Rom";
            opcionMenu.Click += (s, e) => PideRom();
            menuContextual.Items.Add(opcionMenu);
            opcionMenu = new MenuItem();
            opcionMenu.Header = "Hacer BackUp";
            opcionMenu.Click += (s, e) => rom.BackUp();
            menuContextual.Items.Add(opcionMenu);
            opcionMenu = new MenuItem();
            opcionMenu.Header = "Guardar cambios";
            opcionMenu.Click += (s, e) => { rom.Save(); hayCambios = false; };
            menuContextual.Items.Add(opcionMenu);
            ContextMenu = menuContextual;
            PideRom();
            KeyUp += (s, e) =>
            {
                
                if (e.Key == Key.H)
                {
                    huevoActivado = !huevoActivado;
                    PonImagenesMinis();
                }else if(e.Key==Key.S)
                {
                    shinyActivado = !shinyActivado;
                    PonImagenesMinis();
                }
            };


        }

        private void PonImagenesMinis()
        {
            PokemonPokedex[] pokedex;
            pokedex = ugPokedex.Children.OfType<PokemonPokedex>().ToArray();
           
            if (huevoActivado)
            {
                for (int i = 0; i < pokedex.Length; i++)
                {
                    if (!shinyActivado)
                        pokedex[i].imgPokemon.SetImage(Utils.LZ77Handler.ConstructSprite(Resource1.huevo, pokedex[i].Pokemon.ImgFrontal.Paleta));
                    else
                        pokedex[i].imgPokemon.SetImage(Utils.LZ77Handler.ConstructSprite(Resource1.huevo, pokedex[i].Pokemon.PaletaShiny));
                }
            }
            else
            {
                for (int i = 0; i < pokedex.Length; i++)
                {
                    if (!shinyActivado)
                        pokedex[i].imgPokemon.SetImage(pokedex[i].Pokemon.ImgFrontal.ToBitmap());
                    else
                        pokedex[i].imgPokemon.SetImage(pokedex[i].Pokemon.ImgFrontal.ToBitmap(pokedex[i].Pokemon.PaletaShiny));
                }
            }
        }

        private void GuardaRom(object sender, EventArgs e)
        {

            GuardaSiHayCambios();
            Application.Current.Shutdown();
        }

        private void PideRom()
        {
            OpenFileDialog opnRom = new OpenFileDialog();
            PokemonPokedex pokemon;
            FrameWorkPokemonGBA.RomPokemon romCargada;
            opnRom.Filter = "gba|*.gba";
            GuardaSiHayCambios();
            if (opnRom.ShowDialog().Value)
            {
                romCargada = new FrameWorkPokemonGBA.RomPokemon(opnRom.FileName);
                if (!romCargada.EsCompatiblePokedex)
                    MessageBox.Show("La rom no es compatible con el programa");
                else
                {
                    try
                    {

                        ugPokedex.Children.Clear();
                        pokemonActual = null;
                        try
                        {
                            rom = romCargada;
                            cmbObjeto2.ItemsSource = rom.Objetos.ToArray();
                            cmbObjeto1.ItemsSource = rom.Objetos.ToArray();
                            cmbTipo1.ItemsSource = rom.Tipos.ToArray();
                            cmbTipo2.ItemsSource = rom.Tipos.ToArray();
                            

                            for (int i = 0, f = rom.Pokedex.Total; i < f; i++)
                            {

                                    pokemon = new PokemonPokedex(rom.Pokedex[i]);
                                    pokemon.Selected += PonPokemon;
                                    ugPokedex.Children.Add(pokemon);

                            }
                            pokedexCargada = ugPokedex.Children.OfType<PokemonPokedex>().ToArray();
                            PonPokemon(ugPokedex.Children[1] as PokemonPokedex);

                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debugger.Break();
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Problemas al cargar las imagenes!!");
                        if (rom == null) this.Close();

                    }
                }
            }
            else if (rom == null) this.Close();
        }

        public void GuardaSiHayCambios()
        {
            if (hayCambios && rom != null)
                if (MessageBox.Show("Desea guardar los cambios en la rom? ", "Importante", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    GuardaDatosPokemon();
                    rom.Save();

                }
            hayCambios = false;
            
        }

        private void PonPokemon(object sender, EventArgs e = null)
        {

        	Action act;
        	BitmapAnimated bmpAnimated=imgPokemonPokedex.Tag as BitmapAnimated; 
            GuardaDatosPokemon();
            if(bmpAnimated!=null)bmpAnimated.Finsh();
            pokemonActual = sender as PokemonPokedex;
            pltNormal.Colors = pokemonActual.Pokemon.ImgFrontal.Paleta;
            pltShiny.Colors = pokemonActual.Pokemon.PaletaShiny;
            txtNamePokemon.TextChanged -= txtNamePokemon_TextChanged;
            txtNamePokemon.Text = pokemonActual.Pokemon.Nombre;
            txtNamePokemon.TextChanged += txtNamePokemon_TextChanged;
           
            if(rom.Version==FrameWorkPokemonGBA.RomPokemon.VersionRom.Esmeralda){
            	bmpAnimated=pokemonActual.Pokemon.ImgFrontal.ToAnimatedBitmap();
            	bmpAnimated.FrameChanged+=(s,frameActual)=>{
            	act=()=>{           
            		imgPokemonPokedex.SetImage(frameActual);};
            		Dispatcher.BeginInvoke(act);
            	};
            	
            	bmpAnimated.Start();
            	imgPokemonPokedex.Tag=bmpAnimated;
            
            }
            else
                imgPokemonPokedex.SetImage(pokemonActual.Pokemon.ImgFrontal.ToBitmap());
            imgInfoBasicaPkm.SetImage(pokemonActual.Pokemon.ImgFrontal.ToBitmap());
            //descripcion
            try
            {
                txtDescripcion.Text = pokemonActual.Pokemon.PokedexData.Descripcion;
                txtDescripcion.IsReadOnly = false;
            }//lo pongo por si hay problemas al leer la descripción al menos se puede ver :)
            catch { txtDescripcion.Text = "NO SE PUEDE LEER!";txtDescripcion.IsReadOnly = true; }
            //items
            cmbObjeto1.SelectedItem = pokemonActual.Pokemon.Objeto1;
            cmbObjeto2.SelectedItem = pokemonActual.Pokemon.Objeto2;
            //tipos
            cmbTipo1.SelectedItem = pokemonActual.Pokemon.Tipo1;
            cmbTipo2.SelectedItem = pokemonActual.Pokemon.Tipo2;
            //stats
            txtHp.Text = pokemonActual.Pokemon.Hp + "";
            txtAtaque.Text = pokemonActual.Pokemon.Ataque + "";
            txtDefensa.Text = pokemonActual.Pokemon.Defensa + "";
            txtVelocidad.Text = pokemonActual.Pokemon.Velocidad + "";
            txtAtaqueEspecial.Text = pokemonActual.Pokemon.AtaqueEspecial + "";
            txtDefensaEspecial.Text = pokemonActual.Pokemon.DefensaEspecial + "";
            txtExp.Text = pokemonActual.Pokemon.ExpBase + "";
            txtGeneroRatio.Text = pokemonActual.Pokemon.Genero + "";
            txtRatioCaptura.Text = pokemonActual.Pokemon.RatioCaptura + "";
            txtEvs.Text = pokemonActual.Pokemon.Evs + "";
            rbt_Checked();

            //img2
            if (rom.Version == FrameWorkPokemonGBA.RomPokemon.VersionRom.Esmeralda)
            {
                rbt_Checked();
            }else
            {
                imgFrontal2.SetImage(Colors.White.ToBitmap(1, 1));
            }
            hayCambiosPokemonActual = false;
        }

        private void GuardaDatosPokemon()
        {
            if (pokemonActual != null && hayCambiosPokemonActual)
            {
                pokemonActual.Pokemon.Nombre = txtNamePokemon.Text;
                //poner todos los datos!!
              //  pokemonActual.Pokemon.ImgFrontal.Paleta = pltNormal.Colors;
                //pokemonActual.Pokemon.PaletaShiny = pltShiny.Colors;
                
                //descripcion
                if(pokemonActual.Pokemon.PokedexData.Descripcion != txtDescripcion.Text)
                     pokemonActual.Pokemon.PokedexData.Descripcion= txtDescripcion.Text;
                //items
                pokemonActual.Pokemon.Objeto1= cmbObjeto1.SelectedItem as FrameWorkPokemonGBA.Objeto;
                pokemonActual.Pokemon.Objeto2 = cmbObjeto2.SelectedItem as FrameWorkPokemonGBA.Objeto;
                //tipos
                pokemonActual.Pokemon.Tipo1= cmbTipo1.SelectedItem.ToString();//da problemas de momento...
                pokemonActual.Pokemon.Tipo2= cmbTipo2.SelectedItem.ToString();
                //stats
                pokemonActual.Pokemon.Hp = Convert.ToByte(txtHp.Text);
                pokemonActual.Pokemon.Ataque = Convert.ToByte(txtAtaque.Text);
                pokemonActual.Pokemon.Defensa = Convert.ToByte(txtDefensa.Text);
                pokemonActual.Pokemon.Velocidad = Convert.ToByte(txtVelocidad.Text);
                pokemonActual.Pokemon.AtaqueEspecial = Convert.ToByte(txtAtaqueEspecial.Text);
                pokemonActual.Pokemon.DefensaEspecial = Convert.ToByte(txtDefensaEspecial.Text);
                pokemonActual.Pokemon.ExpBase = Convert.ToByte(txtExp.Text);
                pokemonActual.Pokemon.Genero = Convert.ToByte(txtGeneroRatio.Text);
                pokemonActual.Pokemon.RatioCaptura = Convert.ToByte(txtRatioCaptura.Text);
                pokemonActual.Pokemon.Evs = Convert.ToByte(txtEvs.Text);

                hayCambiosPokemonActual = false;
            }
        }

        private void txtNamePokemon_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtNamePokemon.Text.Length > (int)FrameWorkPokemonGBA.Pokemon.LongitudCampos.Nombre)
            {
                e.Handled = true;
                MessageBox.Show("Se ha pasado del máximo de caracteres para el nombre!!");
            }
            else
            {
                txtNamePokemon.Text = txtNamePokemon.Text.ToUpper();
                txtNamePokemon.CaretIndex = txtNamePokemon.Text.Length;
                hayCambios = true;//poner en todos los sitios ;)
                hayCambiosPokemonActual = true;
            }
        }

        private void rbt_Checked(object sender = null, RoutedEventArgs e = null)
        {
            Bitmap bmpImg;
            if (pokemonActual != null)
            {
                if (rbtNormal.IsChecked.Value)
                {
                    bmpImg = pokemonActual.Pokemon.ImgFrontal.ToBitmap(pltNormal.Colors);
                    
                    imgInfoBasicaPkm.SetImage(bmpImg);
                    imgFrontal.SetImage(bmpImg);
                    imgBack.SetImage(pokemonActual.Pokemon.ImgTrasera.ToBitmap(pltNormal.Colors));
                    if(rom.Version==FrameWorkPokemonGBA.RomPokemon.VersionRom.Esmeralda)
                    {
                        imgFrontal2.SetImage(pokemonActual.Pokemon.ImgFrontal.ToBitmap2(pltNormal.Colors));
                    }
                    else
                    {
                        imgPokemonPokedex.SetImage(bmpImg);
                    }
                }
                else
                {
                    imgFrontal.SetImage(pokemonActual.Pokemon.ImgFrontal.ToBitmap(pltShiny.Colors));
                    imgBack.SetImage(pokemonActual.Pokemon.ImgTrasera.ToBitmap(pltShiny.Colors));
                    if (rom.Version == FrameWorkPokemonGBA.RomPokemon.VersionRom.Esmeralda)
                    {
                        imgFrontal2.SetImage(pokemonActual.Pokemon.ImgFrontal.ToBitmap2(pltShiny.Colors));
                    }
                }
            }
        }

        private void plt_ColorChanged(object sender, Gabriel.Cat.Wpf.ColorChangedArgs e)
        {
            if (pltNormal == sender)
            {
                if (rbtNormal.IsChecked.Value)
                {
                    pokemonActual.imgPokemon.SetImage(pokemonActual.Pokemon.ImgFrontal.ToBitmap(pltNormal.Colors));
                    imgPokemonPokedex.Source = pokemonActual.imgPokemon.Source;
                    imgFrontal.Source = pokemonActual.imgPokemon.Source;
                    imgBack.SetImage(pokemonActual.Pokemon.ImgTrasera.ToBitmap(pltNormal.Colors));
                    if (rom.Version == FrameWorkPokemonGBA.RomPokemon.VersionRom.Esmeralda)
                    {
                        imgFrontal2.SetImage(pokemonActual.Pokemon.ImgFrontal.ToBitmap2(pltNormal.Colors));
                    }
                }

            }
            else
            {
                if (!rbtNormal.IsChecked.Value)
                {
                    imgFrontal.SetImage(pokemonActual.Pokemon.ImgFrontal.ToBitmap(pltShiny.Colors));
                    imgBack.SetImage(pokemonActual.Pokemon.ImgTrasera.ToBitmap(pltShiny.Colors));
                    if (rom.Version == FrameWorkPokemonGBA.RomPokemon.VersionRom.Esmeralda)
                    {
                        imgFrontal2.SetImage(pokemonActual.Pokemon.ImgFrontal.ToBitmap2(pltShiny.Colors));
                    }
                }

            }
            hayCambios = true;
            hayCambiosPokemonActual = true;
        }

        private void plt_ColorSelected(object sender, Gabriel.Cat.Wpf.ColorSelectedArgs e)
        {
            colorSelected = e.Color;
        }

        private void txt_KeyDown(object sender, KeyEventArgs e)
        {
            int num;
            hayCambios = true;
            hayCambiosPokemonActual = true;
            try
            {
                e.Handled = !(e.Key >= Key.D0 && e.Key <= Key.D9 || e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9 || e.Key == Key.Decimal||e.Key==Key.Subtract);
                if (!e.Handled)
                {
                    num =int.Parse(((TextBox)sender).Text);
                    e.Handled = num > byte.MaxValue || num < byte.MinValue;
                   if(e.Handled)
                    {
                        if (num < 0) num = 0;
                        ((TextBox)sender).Text = Math.Min(byte.MaxValue, num)+"";
                        e.Handled = false;
                    }
                }
            }
            catch { e.Handled = true; }
            if (e.Handled)
                ((TextBox)sender).Text = ((TextBox)sender).Text.Trim(caracteresNoNumericos);
        }




        private static char[] CaracteresNoNumericos()
        {
            char[] caracteresNoNumericos = new char[byte.MaxValue - 10];
            int pos = 0;
            for (char c = (char)byte.MinValue; c < byte.MaxValue; c++)
                if (!char.IsNumber(c))
                    caracteresNoNumericos[pos++] = c;
            return caracteresNoNumericos;
        }

        private void cmb_Selected(object sender, RoutedEventArgs e)
        {
            hayCambiosPokemonActual = true;
            hayCambios = true;
        }

        private void txtDescripcion_TextChanged(object sender, TextChangedEventArgs e)
        {
            hayCambios = true;
            hayCambiosPokemonActual = true;
        }

        private void txtFiltroNombre_TextChanged(object sender, KeyEventArgs e)
        {
            List<UIElement> pokedexFiltrada = new List<UIElement>();
            string texto;
            if (!String.IsNullOrEmpty(txtFiltroNombre.Text))
            {
                texto = txtFiltroNombre.Text.ToUpper();
                for (int i = 0; i < pokedexCargada.Length; i++)
                {
                    if (pokedexCargada[i].ToString().ToUpper().Contains(texto))
                        pokedexFiltrada.Add(pokedexCargada[i]);
                }
                ugPokedex.Children.Clear();
                ugPokedex.Children.AddRange(pokedexFiltrada);
            }
            else
            {
                ugPokedex.Children.Clear();
                ugPokedex.Children.AddRange(pokedexCargada);
            }
        }
    }
}
