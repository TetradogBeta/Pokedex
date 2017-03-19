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
using PokemonGBAFrameWork;
using System.IO;
using Gabriel.Cat.Extension;
using Gabriel.Cat.Wpf;

namespace Pokedex
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		RomData rom;

		PokemonPokedex pokemonActual;
		BloqueImagen hueboSprite;
		System.Drawing.Color colorSelected;
		bool hayCambios;
		private bool hayCambiosPokemonActual;
		static readonly char[] caracteresNoNumericos = CaracteresNoNumericos();
		bool huevoActivado;
		bool shinyActivado;
		PokemonPokedex[] pokedexCargada;
		public MainWindow()
		{
			int[] nivelEvs = new int[] { 0, 1, 2, 3 };
			ContextMenu menuContextual;
			MenuItem opcionMenu;
			huevoActivado = false;
			hayCambios = false;
			hueboSprite = new BloqueImagen(new BloqueBytes((byte[])Resource1.huevo));
			InitializeComponent();
			evSelectorAtaque.Items.AddRange(nivelEvs);
			evSelectorAtaqueEspecial.Items.AddRange(nivelEvs);
			evSelectorDefensa.Items.AddRange(nivelEvs);
			evSelectorDefensaEspecial.Items.AddRange(nivelEvs);
			evSelectorHp.Items.AddRange(nivelEvs);
			evSelectorVelocidad.Items.AddRange(nivelEvs);
			cmbGrupoHuevo1.ItemsSource = Enum.GetValues(typeof(Pokemon.GrupoHuevo));
			cmbGrupoHuevo2.ItemsSource = Enum.GetValues(typeof(Pokemon.GrupoHuevo));
			cmbCrecimiento.Items.AddRange(Enum.GetValues(typeof(Pokemon.RatioCrecimiento)));
			cmbGenero.Items.AddRange(Enum.GetValues(typeof(Pokemon.RatioGenero)));
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
			opcionMenu.Header = "Hacer BackUp datos en memoria";
			opcionMenu.Click += (s, e) => {
				RomGba romBackUp = new RomGba(new FileInfo(rom.Rom.BackUp()));
				rom.Rom=romBackUp;
				rom.Save();//guardo los datos cambiados en el backup
				
			};
			menuContextual.Items.Add(opcionMenu);
			opcionMenu = new MenuItem();
			opcionMenu.Header = "Guardar cambios";
			opcionMenu.Click += (s, e) => {  rom.Save(); hayCambios = false; };
			menuContextual.Items.Add(opcionMenu);
			ContextMenu = menuContextual;
			PideRom();
			KeyUp += (s, e) =>
			{

				if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
				{
					if (e.Key == Key.H)
					{
						huevoActivado = !huevoActivado;
						PonImagenesMinis();
					}
					else if (e.Key == Key.S)
					{
						shinyActivado = !shinyActivado;
						PonImagenesMinis();
					}
					else if (e.Key == Key.O)
					{
						Pokemon.Orden = (Pokemon.OrdenPokemon)(((int)Pokemon.Orden + 1) % ((int)Pokemon.OrdenPokemon.Nacional + 1));
						ugPokedex.Children.Sort();
					}
				}
			};

			KeyDown += ControlTeclasDown;

		}

		private void ControlTeclasDown(object sender, KeyEventArgs e)
		{

			int indexPokemonHaCargar = -1;
			//para pasar los pokemons :D
			//no funciona en todos los casos...
			if (e.Key == Key.Down)
			{
				indexPokemonHaCargar = ugPokedex.Children.IndexOf(pokemonActual) + 1;

			}
			else if (e.Key == Key.Up)
			{
				indexPokemonHaCargar = ugPokedex.Children.IndexOf(pokemonActual) - 1;
				if (indexPokemonHaCargar < 0) indexPokemonHaCargar = 0;
			}
			if (indexPokemonHaCargar >= 0 && indexPokemonHaCargar < ugPokedex.Children.Count)
				PonPokemon(ugPokedex.Children[indexPokemonHaCargar]);


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
						pokedex[i].imgPokemon.SetImage(hueboSprite+pokedex[i].Pokemon.Sprites.PaletaNormal);
					else
						pokedex[i].imgPokemon.SetImage(hueboSprite+pokedex[i].Pokemon.Sprites.PaletaShiny);
				}
			}
			else
			{
				for (int i = 0; i < pokedex.Length; i++)
				{
					if (!shinyActivado)
						pokedex[i].imgPokemon.SetImage(pokedex[i].Pokemon.Sprites.SpritesFrontales[0]);
					else
						pokedex[i].imgPokemon.SetImage(pokedex[i].Pokemon.Sprites.SpritesFrontales[0][1]);
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
			RomGba romCargada;

			opnRom.Filter = "gba|*.gba";
			GuardaSiHayCambios();
			if (opnRom.ShowDialog().Value)
			{

				romCargada = new RomGba(new FileInfo(opnRom.FileName));

				try
				{
					ugPokedex.BeginInit();
					ugPokedex.Children.Clear();
					pokemonActual = null;
					try
					{

						
						Title = "Pokédex -" + romCargada.Nombre;
						rom =new RomData(romCargada);//772 milisegundos
						cmbHabiliad1.ItemsSource = rom.Habilidades;
						cmbHabiliad2.ItemsSource = rom.Habilidades;

						
						cmbTipo1.ItemsSource = rom.Tipos;
						cmbTipo2.ItemsSource = rom.Tipos;
						cmbObjeto1.Items.Clear();
						for (int i = 0; i < rom.Objetos.Count; i++)
						{
							cmbObjeto1.Items.Add(new ObjetoViewer(rom.Objetos[i]));
						}
						cmbObjeto2.ItemsSource = cmbObjeto1.Items;

						try
						{
							//missigno es un pokemon especial porque el orden nacional no tiene...y coge el de Mew...y para poderlo tener correctamente lo pongo a mano
							pokemon = new PokemonPokedex(rom.Pokedex[0]);//para coger la pokedex se usa el orden nacional no el de la gameFreak
							pokemon.Selected += PonPokemon;
							pokemon.Pokemon.Descripcion = DescripcionPokedex.GetDescripcionPokedex(rom, 0);
							pokemon.Pokemon.OrdenNacional = 0;//le pongo el orden que le toca porque de forma auto coge el de mew...
							ugPokedex.Children.Add(pokemon);
						}
						catch { }
						for (int i = 1, f = rom.Pokedex.Count; i < f; i++)
						{
							try
							{
								if (rom.Pokedex[i].Descripcion!=null)
								{
									pokemon = new PokemonPokedex(rom.Pokedex[i]);
									pokemon.Selected += PonPokemon;
									ugPokedex.Children.Add(pokemon);
								}
							}
							catch { System.Diagnostics.Debugger.Break(); }
						}
						
						pokedexCargada = ugPokedex.Children.OfType<PokemonPokedex>().ToArray();
						ugPokedex.Children.Sort();
						PonPokemon(pokedexCargada[0]);
					}
					catch (Exception ex)
					{
						System.Diagnostics.Debugger.Break();
					}
					finally
					{
						ugPokedex.EndInit();
						
					}
				}
				catch
				{
					MessageBox.Show("Problemas al cargar las imagenes!!");
					if (rom == null) this.Close();

				}

			}
			else if (rom == null) this.Close();
		}

		public void GuardaSiHayCambios()
		{

			if (hayCambios && rom != null)
				if (MessageBox.Show("Desea guardar los cambios en la rom? ", "Importante", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
			{
				ActualizoDatosPokemon();
				rom.Save();
				

			}
			hayCambios = false;

		}

		private void PonPokemon(object sender, EventArgs e = null)
		{

			Action act;
			BitmapAnimated bmpAnimated = imgPokemonPokedex.Tag as BitmapAnimated;
			ActualizoDatosPokemon();
			if (bmpAnimated != null) bmpAnimated.Finsh();
			pokemonActual = sender as PokemonPokedex;
			pltNormal.Colors = pokemonActual.Pokemon.Sprites.PaletaNormal.Colores;
			pltShiny.Colors = pokemonActual.Pokemon.Sprites.PaletaShiny.Colores;
			txtNamePokemon.TextChanged -= txtNamePokemon_TextChanged;
			txtNamePokemon.Text = pokemonActual.Pokemon.Nombre;
			txtNamePokemon.TextChanged += txtNamePokemon_TextChanged;
			evSelectorAtaque.SelectedIndex = (int)pokemonActual.Pokemon.AtaqueEvs;
			evSelectorHp.SelectedIndex = (int)pokemonActual.Pokemon.HpEvs;
			evSelectorVelocidad.SelectedIndex = (int)pokemonActual.Pokemon.VelocidadEvs;
			evSelectorDefensa.SelectedIndex = (int)pokemonActual.Pokemon.DefensaEvs;
			evSelectorAtaqueEspecial.SelectedIndex = (int)pokemonActual.Pokemon.AtaqueEspecialEvs;
			evSelectorDefensaEspecial.SelectedIndex = (int)pokemonActual.Pokemon.DefensaEspecialEvs;
			txtNombreEspecie.TextChanged -= txtNombreEspecie_TextChanged;
			if(pokemonActual.Pokemon.Huella!=null)
				imgHuella.SetImage(pokemonActual.Pokemon.Huella.GetImagen());
			if (pokemonActual.Pokemon.Descripcion != null)
			{
				txtNombreEspecie.Text = pokemonActual.Pokemon.Descripcion.Especie;
				txtAltura.Text = (pokemonActual.Pokemon.Descripcion.Altura/10.0)+"";
				txtPeso.Text = (pokemonActual.Pokemon.Descripcion.Peso / 10.0) + "";
			}
			else
			{
				txtNombreEspecie.Text = "No tiene...";
				txtAltura.Text = "";
				txtPeso.Text = "";
			}
			txtNombreEspecie.IsReadOnly = pokemonActual.Pokemon.Descripcion == null;
			txtAltura.IsReadOnly = pokemonActual.Pokemon.Descripcion == null;
			txtPeso.IsReadOnly = pokemonActual.Pokemon.Descripcion == null;
			txtNombreEspecie.TextChanged += txtNombreEspecie_TextChanged;

			bmpAnimated = pokemonActual.Pokemon.Sprites.GetAnimacionImagenFrontal();
			bmpAnimated.FrameChanged += (s, frameActual) =>
			{
				act = () =>
				{
					imgPokemonPokedex.SetImage(frameActual);
				};
				Dispatcher.BeginInvoke(act);
			};

			bmpAnimated.Start();
			imgPokemonPokedex.Tag = bmpAnimated;


			imgInfoBasicaPkm.SetImage(pokemonActual.Pokemon.Sprites.SpritesFrontales[0]);
			//descripcion
			txtDescripcion2.Text = "";
			txtDescripcion.TextChanged -= txtDescripcion_TextChanged;
			txtDescripcion2.TextChanged -= txtDescripcion_TextChanged;
			if (pokemonActual.Pokemon.Descripcion != null)
			{
				txtDescripcion.Text = pokemonActual.Pokemon.Descripcion.Descripcion;

			}
			else
				txtDescripcion.Text = "No tiene descripcion en la pokedex...";

			txtDescripcion.IsReadOnly = pokemonActual.Pokemon.Descripcion == null;

			txtDescripcion.TextChanged += txtDescripcion_TextChanged;

			//items
			cmbObjeto1.SelectedIndex = pokemonActual.Pokemon.Objeto1;
			cmbObjeto2.SelectedIndex = pokemonActual.Pokemon.Objeto2;
			//grupo huevo
			cmbGrupoHuevo1.SelectedIndex =(int) pokemonActual.Pokemon.GrupoHuevo1;
			cmbGrupoHuevo2.SelectedIndex = (int)pokemonActual.Pokemon.GrupoHuevo2;
			//tipos
			cmbTipo1.SelectedIndex = pokemonActual.Pokemon.Tipo1;
			cmbTipo2.SelectedIndex = pokemonActual.Pokemon.Tipo2;
			//habilidades
			cmbHabiliad1.SelectedIndex = pokemonActual.Pokemon.Habilidad1;
			cmbHabiliad2.SelectedIndex = pokemonActual.Pokemon.Habilidad2;
			//stats
			txtHp.Text = pokemonActual.Pokemon.Hp + "";
			txtAtaque.Text = pokemonActual.Pokemon.Ataque + "";
			txtDefensa.Text = pokemonActual.Pokemon.Defensa + "";
			txtVelocidad.Text = pokemonActual.Pokemon.Velocidad + "";
			txtAtaqueEspecial.Text = pokemonActual.Pokemon.AtaqueEspecial + "";
			txtDefensaEspecial.Text = pokemonActual.Pokemon.DefensaEspecial + "";
			cmbCrecimiento.SelectedIndex = cmbCrecimiento.Items.IndexOf(pokemonActual.Pokemon.Crecimiento);
			cmbGenero.SelectedIndex = cmbGenero.Items.IndexOf(pokemonActual.Pokemon.RatioSexo);
			txtRatioCaptura.Text = pokemonActual.Pokemon.RatioCaptura + "";
			rbt_Checked();

			lstAtaques.Items.Clear();
			for (int i = 0; i < pokemonActual.Pokemon.AtaquesAprendidos.Ataques.Count; i++)
				lstAtaques.Items.Add(new AtaqueAprendidoViewer(rom, pokemonActual.Pokemon, i));

			hayCambiosPokemonActual = false;
		}

		private void ActualizoDatosPokemon()
		{
			if (pokemonActual != null && hayCambiosPokemonActual)
			{
				pokemonActual.Pokemon.Nombre.Texto = txtNamePokemon.Text;
				//poner todos los datos!!
				pokemonActual.Pokemon.Sprites.PaletaNormal.Colores.SetIList(pltNormal.Colors);
				pokemonActual.Pokemon.Sprites.PaletaShiny.Colores.SetIList(pltShiny.Colors);
				pokemonActual.Pokemon.AtaqueEvs = (Pokemon.NivelEvs)evSelectorAtaque.SelectedIndex;
				pokemonActual.Pokemon.HpEvs = (Pokemon.NivelEvs)evSelectorHp.SelectedIndex;
				pokemonActual.Pokemon.VelocidadEvs = (Pokemon.NivelEvs)evSelectorVelocidad.SelectedIndex;
				pokemonActual.Pokemon.DefensaEvs = (Pokemon.NivelEvs)evSelectorDefensa.SelectedIndex;
				pokemonActual.Pokemon.AtaqueEspecialEvs = (Pokemon.NivelEvs)evSelectorAtaqueEspecial.SelectedIndex;
				pokemonActual.Pokemon.DefensaEspecialEvs = (Pokemon.NivelEvs)evSelectorDefensaEspecial.SelectedIndex;
				//descripcion
				if (pokemonActual.Pokemon.Descripcion.Descripcion != txtDescripcion.Text)
					pokemonActual.Pokemon.Descripcion.Descripcion = txtDescripcion.Text;
				if (pokemonActual.Pokemon.Descripcion.Especie != txtNombreEspecie.Text)
					pokemonActual.Pokemon.Descripcion.Especie = txtNombreEspecie.Text;
				//grupo huevo
				pokemonActual.Pokemon.GrupoHuevo1 = (Pokemon.GrupoHuevo)cmbGrupoHuevo1.SelectedIndex;
				pokemonActual.Pokemon.GrupoHuevo2 = (Pokemon.GrupoHuevo)cmbGrupoHuevo2.SelectedIndex;
				//items

				pokemonActual.Pokemon.Objeto1 = cmbObjeto1.SelectedIndex;
				pokemonActual.Pokemon.Objeto2 = cmbObjeto2.SelectedIndex;
				pokemonActual.Pokemon.SetObjetosEnLosStats(cmbObjeto1.Items.Count);
				pokemonActual.Pokemon.Tipo1 = (byte)cmbTipo1.SelectedIndex;
				pokemonActual.Pokemon.Tipo2 = (byte)cmbTipo2.SelectedIndex;
				pokemonActual.Pokemon.Habilidad1 = (byte)cmbHabiliad1.SelectedIndex;
				pokemonActual.Pokemon.Habilidad2 = (byte)cmbHabiliad2.SelectedIndex;

				//stats
				pokemonActual.Pokemon.Hp = Convert.ToByte(txtHp.Text);
				pokemonActual.Pokemon.Ataque = Convert.ToByte(txtAtaque.Text);
				pokemonActual.Pokemon.Defensa = Convert.ToByte(txtDefensa.Text);
				pokemonActual.Pokemon.Velocidad = Convert.ToByte(txtVelocidad.Text);
				pokemonActual.Pokemon.AtaqueEspecial = Convert.ToByte(txtAtaqueEspecial.Text);
				pokemonActual.Pokemon.DefensaEspecial = Convert.ToByte(txtDefensaEspecial.Text);
				pokemonActual.Pokemon.Crecimiento = (Pokemon.RatioCrecimiento)cmbCrecimiento.SelectedIndex;
				pokemonActual.Pokemon.RatioSexo = (Pokemon.RatioGenero)Convert.ToInt32(cmbGenero.SelectedIndex);
				pokemonActual.Pokemon.RatioCaptura = Convert.ToByte(txtRatioCaptura.Text);


				hayCambiosPokemonActual = false;
			}
		}

		private void txtNamePokemon_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (txtNamePokemon.Text.Length > (int)Pokemon.LongitudCampos.Nombre)
			{
				e.Handled = true;
				MessageBox.Show("Se ha pasado del máximo de caracteres para el nombre!!");
			}
			else
			{
				txtNamePokemon.Text = txtNamePokemon.Text;
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
					bmpImg = pokemonActual.Pokemon.Sprites.SpritesFrontales[0]+new Paleta(pltNormal.Colors); 
					imgInfoBasicaPkm.SetImage(bmpImg);
					imgPokemonMasInfo.SetImage(bmpImg);

					if (rom.Edicion.AbreviacionRom != AbreviacionCanon.BPE)
					{
						
						imgPokemonPokedex.SetImage(bmpImg);
					}
				}
				//pongo las imgs
				PonImagenes(rbtNormal.IsChecked.Value ? pltNormal : pltShiny);

			}
		}

		private void plt_ColorChanged(object sender, Gabriel.Cat.Wpf.ColorChangedArgs e)
		{
			
			ColorTable plt = sender as ColorTable;
			if (pltNormal == sender)
			{
				if (rbtNormal.IsChecked.Value)
				{
					pokemonActual.imgPokemon.SetImage(pokemonActual.Pokemon.Sprites.SpritesFrontales[0]+ new Paleta(pltNormal.Colors));
					imgPokemonPokedex.Source = pokemonActual.imgPokemon.Source;
					imgPokemonMasInfo.Source = pokemonActual.imgPokemon.Source;
					imgInfoBasicaPkm.Source = pokemonActual.imgPokemon.Source;
					
				}

			}
			//pongo las imgs
			PonImagenes(plt);
			hayCambios = true;
			hayCambiosPokemonActual = true;
		}
		private void PonImagenes(ColorTable paleta)
		{
			System.Windows.Controls.Image img;
			ugImgs.Children.Clear();
			for (int i = 0; i < pokemonActual.Pokemon.Sprites.SpritesFrontales.Count; i++)
			{
				img = new System.Windows.Controls.Image();
				img.SetImage(pokemonActual.Pokemon.Sprites.SpritesFrontales[i] +new Paleta(paleta.Colors));
				ugImgs.Children.Add(img);
			}
			for (int i = 0; i < pokemonActual.Pokemon.Sprites.SpritesTraseros.Count; i++)
			{
				img = new System.Windows.Controls.Image();
				img.SetImage(pokemonActual.Pokemon.Sprites.SpritesTraseros[i] + new Paleta(paleta.Colors));
				ugImgs.Children.Add(img);
			}
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
				e.Handled = !(e.Key >= Key.D0 && e.Key <= Key.D9 || e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9 || e.Key == Key.Decimal || e.Key == Key.Subtract);
				if (!e.Handled)
				{
					num = int.Parse(((TextBox)sender).Text);
					e.Handled = num > byte.MaxValue || num < byte.MinValue;
					if (e.Handled)
					{
						if (num < 0) num = 0;
						((TextBox)sender).Text = Math.Min(byte.MaxValue, num) + "";
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

		private void txtNombreEspecie_TextChanged(object sender, TextChangedEventArgs e)
		{
			hayCambios = true;
			hayCambiosPokemonActual = true;
			if (txtNombreEspecie.Text.Length > (int)DescripcionPokedex.LongitudCampos.NombreEspecie)
				txtNombreEspecie.Text = txtNombreEspecie.Text.Substring(0, (int)DescripcionPokedex.LongitudCampos.NombreEspecie);
		}
	}
}
