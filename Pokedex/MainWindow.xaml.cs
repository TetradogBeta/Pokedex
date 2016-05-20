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
        public MainWindow()
        {
            ContextMenu menuContextual;
            MenuItem opcionMenu;
            hayCambios = false;
            InitializeComponent();
            pltNormal.ColorPicker.IsAlfaEnabled = false;
            pltShiny.ColorPicker.IsAlfaEnabled = false;

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
            opcionMenu.Click += (s, e) => { rom.Save();hayCambios = false; };
            menuContextual.Items.Add(opcionMenu);
            ContextMenu = menuContextual;
            PideRom();
            Closed += GuardaRom;
        }

        private void GuardaRom(object sender, EventArgs e)
        {
            GuardaSiHayCambios();
                
        }

        private void PideRom()
        {
            OpenFileDialog opnRom = new OpenFileDialog();
            PokemonPokedex pokemon;
            FrameWorkPokemonGBA.RomPokemon romCargada;
            opnRom.Filter = "gba|*.gba";
            GuardaSiHayCambios();
            if(opnRom.ShowDialog().Value)
            {
                romCargada= new FrameWorkPokemonGBA.RomPokemon(opnRom.FileName);
                if (!romCargada.EsCompatiblePokedex)
                    MessageBox.Show("La rom no es compatible con el programa");
                else
                {
                    try
                    {
                        pokemon = new PokemonPokedex(romCargada.Pokedex[0]);
                        ugPokedex.Children.Clear();
                        try
                        {
                            rom = romCargada;
                            PonPokemon(pokemon);
                            pokemon.Selected += PonPokemon;
                            ugPokedex.Children.Add(pokemon);
                            for (int i = 1, f = rom.Pokedex.Total; i < f; i++)
                            {
                                pokemon = new PokemonPokedex(rom.Pokedex[i]);
                                pokemon.Selected += PonPokemon;
                                ugPokedex.Children.Add(pokemon);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debugger.Break();
                        }
                    }catch
                    {
                        MessageBox.Show("Problemas al cargar las imagenes!!");
                        if (rom == null) this.Close();

                    }
                }
            }
            else if(rom==null) this.Close();
        }

        public void GuardaSiHayCambios()
        {
            if (hayCambios)
                if (MessageBox.Show("Desea guardar los cambios en la rom? ", "Importante", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    GuardaDatosPokemon();
                    rom.Save();
                }
        }

        private void PonPokemon(object sender, EventArgs e=null)
        {
            
            GuardaDatosPokemon();
            pokemonActual = sender as PokemonPokedex;
             pltNormal.Colors = pokemonActual.Pokemon.ImgFrontal.Paleta;
            pltShiny.Colors = pokemonActual.Pokemon.ImgFrontalShiny.Paleta;
            txtNamePokemon.Text = pokemonActual.Pokemon.Nombre;
            
            imgPokemonPokedex.SetImage(pokemonActual.Pokemon.ImgFrontal.ToBitmap());
           
            rbt_Checked();
        }

        private void GuardaDatosPokemon()
        {
            if(pokemonActual!=null)
            {
                pokemonActual.Pokemon.Nombre = txtNamePokemon.Text;
                //poner todos los datos!!
                pokemonActual.Pokemon.ImgFrontal.Paleta = pltNormal.Colors;
                pokemonActual.Pokemon.ImgFrontalShiny.Paleta = pltShiny.Colors;
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

            }
        }

        private void rbt_Checked(object sender=null, RoutedEventArgs e=null)
        {
            Bitmap bmpImg;
            if (pokemonActual != null)
            {
                if (rbtNormal.IsChecked.Value)
                {
                    bmpImg = pokemonActual.Pokemon.ImgFrontal.ToBitmap(pltNormal.Colors);
                    imgPokemonPokedex.SetImage(bmpImg);
                    imgFrontal.SetImage(bmpImg);
                    imgBack.SetImage(pokemonActual.Pokemon.ImgTrasera.ToBitmap(pltNormal.Colors));
                }
                else
                {
                    imgFrontal.SetImage(pokemonActual.Pokemon.ImgFrontalShiny.ToBitmap(pltShiny.Colors));
                    imgBack.SetImage(pokemonActual.Pokemon.ImgTraseraShiny.ToBitmap(pltShiny.Colors));
                }
            }
        }

        private void plt_ColorChanged(object sender, Gabriel.Cat.Wpf.ColorChangedArgs e)
        {
            if(pltNormal==sender)
            {
                if (rbtNormal.IsChecked.Value)
                {
                    pokemonActual.imgPokemon.SetImage(pokemonActual.Pokemon.ImgFrontal.ToBitmap(pltNormal.Colors));
                    imgPokemonPokedex.Source = pokemonActual.imgPokemon.Source;
                    imgFrontal.Source = pokemonActual.imgPokemon.Source;
                    imgBack.SetImage(pokemonActual.Pokemon.ImgTrasera.ToBitmap(pltNormal.Colors));
                }

            }
            else
            {
                if (!rbtNormal.IsChecked.Value)
                {
                    imgFrontal.SetImage(pokemonActual.Pokemon.ImgFrontalShiny.ToBitmap(pltShiny.Colors));
                    imgBack.SetImage(pokemonActual.Pokemon.ImgTraseraShiny.ToBitmap(pltShiny.Colors));
                }

            }
            hayCambios = true;

        }

        private void plt_ColorSelected(object sender, Gabriel.Cat.Wpf.ColorSelectedArgs e)
        {
            colorSelected = e.Color;
        }
    }
}
