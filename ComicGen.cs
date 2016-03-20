using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using Newtonsoft.Json;

namespace ComicGenerator
{
	class ComicGen
	{
		Random m_rand;

		public class Message
		{
			public Message ( ulong a_user, string a_text )
			{
				User = a_user;
				Text = a_text;
			}

			public ulong User { get; set; }
			public string Text { get; set; }
		}

		public MemoryStream Generate ( string a_packName, List<Message> a_messages )
		{
			m_rand = new Random ( (int)DateTime.Now.Ticks );

			// Currently hardcoded because I'm lazy :P
			string[] packs = new string[] { "nik", "juice" };

			bool random = a_packName == "random";
			string packDir = "comic";
						
			List<PanelDef> panDefs = null;
			try
			{
				if ( !random )
				{
					if ( a_packName == null || a_packName == "" )
					{
						string packName = packs[m_rand.Next ( packs.Length)];
						panDefs = LoadPanels ( Path.Combine ( packDir, packName ) );
					}
					else
					{
						panDefs = LoadPanels ( Path.Combine ( packDir, a_packName ) );
					}
				}
				else
				{
					panDefs = new List<PanelDef> ();
					foreach ( var name in packs )
					{
						var tmp = LoadPanels ( Path.Combine ( packDir, name ) );
						panDefs.AddRange ( tmp );
					}
				}
			}
			catch ( IOException e )
			{
				Console.WriteLine ( "ERR: Couldn't load panels.json." );
				Console.WriteLine ( e );
				return null;
			}
			
			//a_messages.Sort ( ( a, b ) => b.Timestamp.CompareTo ( a.Timestamp ) );

			// Generate the strip and send it.
			var panels = GeneratePanels ( m_rand, panDefs, a_messages );
			var bmp = GenerateComic ( m_rand, panels );
			return StreamFromBitmap ( bmp );
		}

		public class PanelDef
		{
			public PanelDef ( Bitmap a_bmp, Rectangle[] a_rects )
			{
				Bmp = a_bmp;
				Rects = a_rects;
			}

			public Bitmap Bmp { get; set; }
			public Rectangle[] Rects { get; set; }
		};

		public class PanelConfig
		{
			public class Panel
			{
				public string Path { get; set; }
				public int[][] Rects { get; set; }
			}

			public List<Panel> Panels { get; set; }
		}

		List<PanelDef> LoadPanels ( string a_path )
		{
			using ( var sr = new StreamReader ( File.Open ( Path.Combine ( a_path, "panels.json" ), FileMode.Open, FileAccess.Read ) ) )
			{
				var conf = JsonConvert.DeserializeObject<PanelConfig> ( sr.ReadToEnd () );

				var panels = new List<PanelDef> ();
				foreach ( var panel in conf.Panels )
				{
					// Read bitmap.
					string path = Path.Combine ( a_path, panel.Path );
					if ( !File.Exists ( path ) )
					{
						throw new InvalidDataException ();
					}
					Bitmap bmp = new Bitmap ( path );

					// Read text rectangles.
					Rectangle[] rects = new Rectangle[panel.Rects.Length];
					for ( int i = 0; i < panel.Rects.Length; ++i )
					{
						// Convert int array to rectangle structures.
						int[] rect = panel.Rects[i];
						if ( rect.Length != 4 )
						{
							throw new InvalidDataException ();
						}
						rects[i] = new Rectangle ( rect[0], rect[1], rect[2], rect[3] );
					}

					// Read
					panels.Add ( new PanelDef ( bmp, rects ) ); 
				}

				return panels;
			}
		}

		class Panel
		{
			public PanelDef PanelDef { get; set; }
			public Message[] Messages { get; set; }

		}

		List<Panel> GeneratePanels ( Random a_rand, List<PanelDef> a_panelDefs, List<Message> a_messages )
		{
			List<Panel> panels = new List<Panel> ();
			
			for ( int i = 0; i < a_messages.Count; )
			{
				PanelDef def = null;
				while ( def == null || ( i + def.Rects.Length ) > a_messages.Count )
				{
					def = a_panelDefs[a_rand.Next ( a_panelDefs.Count () )];
				}

				Panel pan = new Panel ();
				pan.PanelDef = def;
				pan.Messages = new Message[def.Rects.Length];
				for ( int j = 0; j < def.Rects.Length; ++j )
				{
					pan.Messages[j] = a_messages[i++];
				}

				panels.Add ( pan );
			}

			return panels;
		}

		readonly int spacing	= 16;
		readonly int panWidth	= 256;
		readonly int panHeight	= 192;

		Bitmap GenerateComic ( Random a_rand, List<Panel> a_panels )
		{
			int width	= spacing + ( panWidth + spacing ) * Math.Min ( a_panels.Count, 3 );
			int height	= spacing + ( panHeight + spacing ) * ( ( a_panels.Count - 1 ) / 3 + 1 );

			Bitmap bmp = new Bitmap ( width, height, PixelFormat.Format24bppRgb );
			using ( Graphics g = Graphics.FromImage ( bmp ) )
			{
				g.Clear ( Color.FromArgb ( a_rand.Next ( Int32.MinValue, Int32.MaxValue ) ) );

				var userColours = new Dictionary<ulong, Color> ();

				// Draw each panel.
				Font font = new Font ( "Comic Sans MS", 8, FontStyle.Regular );
				for ( int i = 0; i < a_panels.Count; ++i )
				{
					Point offset = new Point (
							spacing + ( panWidth + spacing ) * ( i % 3 ),
							spacing + ( panHeight + spacing ) * ( i / 3 )
						);

					Panel panel = a_panels[i];
					PanelDef def = panel.PanelDef;

					DrawPanel ( g, def.Bmp, offset, 2 );
					for ( int j = 0; j < def.Rects.Length; ++j )
					{
						var rect = def.Rects[j];
						var msg = panel.Messages[j];

						// Get text colour from user.
						Color colour;
						if ( userColours.ContainsKey ( msg.User ) )
						{
							colour = userColours[msg.User];
						}
						else
						{
							colour = Color.FromArgb ( 0xFF,
									m_rand.Next ( 0x7F ),
									m_rand.Next ( 0x7F ),
									m_rand.Next ( 0x7F )
								);
							userColours[msg.User] = colour;
						}

						// Draw 
						Rectangle offRect = rect;
						offRect.Offset ( offset );
						using ( SolidBrush brush = new SolidBrush ( colour ) )
						{
							g.DrawString ( msg.Text, font, brush, offRect );
						}
					}
				}
			}

			return bmp;
		}

		void DrawPanel ( Graphics a_g, Bitmap a_panel, Point a_pos, int a_borderWidth )
		{
			a_g.DrawRectangle ( new Pen ( Color.Black, a_borderWidth ), a_pos.X - 1, a_pos.Y - 1, panWidth + 2, panHeight + 2 );
			a_g.DrawImage ( a_panel, a_pos.X, a_pos.Y, panWidth, panHeight );
		}

		public static MemoryStream StreamFromBitmap ( Bitmap a_bmp )
		{
			var ms = new MemoryStream ();
			a_bmp.Save ( ms, ImageFormat.Png );
			ms.Seek ( 0, SeekOrigin.Begin );

			return ms;
		}
	}
}
