using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicGenerator
{
	class Program
	{
		static void Main ( string[] a_args )
		{
			string outPath = null;
			string packName = null;
			var messages = new List<ComicGen.Message> ();
			ulong userId = 0;

			bool readUser = true;
			foreach ( var arg in a_args )
			{
				if ( outPath == null )
				{
					outPath = arg;
					continue;
				}

				if ( packName == null )
				{
					packName = arg;
					continue;
				}

				if ( readUser )
				{
					try
					{
						userId = Convert.ToUInt64 ( arg );
					}
					catch
					{
						Console.WriteLine ( "ERR: Arg isn't a number." );
						Environment.Exit ( -1 );
					}
					readUser = false;
				}
				else
				{
					var msg = new ComicGen.Message ( userId, arg );
					messages.Add ( msg ); 
					readUser = true;
				}
			}

			if ( outPath == null || packName == null || messages.Count == 0 )
			{
				Console.WriteLine ( "ERR: Bag arg ranges." );
				Environment.Exit ( -1 );
			}

			ComicGen gen = new ComicGen ();
			var ms = gen.Generate ( packName, messages );
			if ( ms == null )
			{
				Console.WriteLine ( "ERR: Failed to generate comic." );
				Environment.Exit ( -2 );
			}

			try
			{
				var fs = File.Open ( outPath, FileMode.Create );
				ms.CopyTo ( fs );
				ms.Close ();
				fs.Close ();
			}
			catch ( IOException e )
			{
				Console.WriteLine ( "ERR: Failed to write comic to disk." );
				Console.WriteLine ( e );
			}
		}
	}
}
