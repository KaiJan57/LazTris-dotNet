using System;
//not required without audio support
//using System.Media;
//include csvorbis to enable audio

namespace LazTris
{
	class MainClass
	{
		public struct BufferObject
        {
            //a simple struct that holds a color and a string two chars in length
            public BufferObject(ConsoleColor color, string chars)
			{
				Color = color;
				Enabled = true;
				Chars = chars;
			}
			public ConsoleColor Color;
			public bool Enabled;
			public string Chars;
		}

		static PlayField field;
		static DateTime update;
		static BufferObject[,] buffer;
		static BufferObject[,] nextbuffer;
		static BufferObject[,] databuffer;
		static int nextbufferwidth = 0;
		static int nextbufferheight = 0;
		static readonly string pixel = "[]";
		static readonly string blank = "  ";
		static readonly string playfieldbackground = " ▒";
		static readonly string border = "▓▓";
		static readonly int Width = 10, Height = 20;
		static readonly int borderwidth = 1;
		static readonly int resumedelay = 500;
		static readonly ConsoleColor foregroundcolor = ConsoleColor.Green;

		//requires csvorbis
		//static SoundPlayer music;

		static float tickdelay = 500;
		static float dropdelay = 100;
		static int linescompleted = 0;
		static int initlevel = 0;
		static int score = 0;

		static bool dropping;

		public static void Main (string[] args)
		{
			//Hide cursor
			Console.CursorVisible = false;

			//Set foreground color
			Console.ForegroundColor = foregroundcolor;

			//Load music
			//Console.WriteLine("Loading...");
			//music = new SoundPlayer(new OggDecodeStream(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tetris.tetrishero.ogg")));
			//Reset input stream because oggdecoder messes it up
            Console.SetIn(Console.In);

			//Get maximum width and height of parts
			for (int i = 0; i < Part.Manager.GetPartCount(); i++)
			{
				Part check = Part.Manager.GetPart(i);
				if (nextbufferwidth < check.Width)
				{
					nextbufferwidth = check.Width;
				}
				if (nextbufferheight< check.Height)
				{
					nextbufferheight = check.Height;
				}
			}

			//Add spacing so the displayed part wont touch the display's border
			nextbufferwidth += 2;
			nextbufferheight += 2;
			//Reset dynamic variables
			InitializeGame();

			//Main loop
			while (true)
			{
				//Check if the playfield should be updated
				if (update <= DateTime.UtcNow)
				{
					//calculate next delays
					tickdelay = CalculateDelay();
					dropdelay = tickdelay/10;
					//Prepare next time check
					update = update.AddMilliseconds(dropping ? dropdelay : tickdelay);
					if (dropping)
					{
						score += 2;
					}
					//Update the playfield
					field.Tick();
					//Draw whole field and falling parts to buffer, but only draw falling part if game is not over
					Draw(field.Lost());
					//Print the Buffer's contents to screen
					PrintBuffer();
					//disable drop mode if part can't fall anymore
					if (!field.CanFall())
					{
						dropping = false;
					}
				}
				//Check for user input
				if (Console.KeyAvailable)
				{
					switch (Console.ReadKey(true).Key)
					{
					case ConsoleKey.UpArrow:
					case ConsoleKey.W:
						{
							//Rotate (Right)
							field.Rotate(Part.RotationDirection.Right);
							break;
						}
					case ConsoleKey.LeftArrow:
					case ConsoleKey.A:
						{
							//Move left
							field.Move(-1);
							break;
						}
					case ConsoleKey.RightArrow:
					case ConsoleKey.D:
						{
							//Move right
							field.Move(1);
							break;
						}
					case ConsoleKey.DownArrow:
					case ConsoleKey.S:
						{
							//Soft-drop (update playfield once per keypress)
							field.Tick();
							//Soft-drop award
							score ++;
							break;
						}
					case ConsoleKey.Enter:
					case ConsoleKey.Tab:
						{
							/*
							//Drop part
							while (field.CanFall())
							{
								//update playfield, draw to screen and wait 10 ms if the falling part can fall
								field.Tick();
								//Drop award
								score += 2;
								Draw(!field.Lost());
								PrintBuffer();
								System.Threading.Thread.Sleep(dropdelay);
							}
							//Clear input buffer (keypresses that were written into the input stream while dropping the part)
							while (Console.KeyAvailable)
							{
								Console.Read();
							}*/
							//Now the part lies on other parts or on the floor and is not locked
							//Cause an update to lock the part after 50 ms
							dropping = !dropping;
							if (dropping)
							{
								update = DateTime.UtcNow;
							}
							break;
						}
					case ConsoleKey.Spacebar:
						{
							TimeSpan timeshift = DateTime.UtcNow.Subtract(update);

							//Pause the game
							Console.WriteLine("Game is paused. Press space to resume.");
							//Stop music
							//music.Stop();
							//While game is paused, code execution is stuck in this loop until it is broken by the space bar
							while (Console.ReadKey(true).Key != ConsoleKey.Spacebar) { }
                                //Clear the text
                                Console.Clear();
							//Start music
							//music.PlayLooping();
							update = DateTime.UtcNow.Add(timeshift);
							break;
						}
					}
					//Bring the current part's moves and rotations to screen
					Draw(field.Lost());
					PrintBuffer();
				}
			}
		}

		static void InitializeGame()
		{
			//Stop music before playing game
			//music.Stop();
			//Wait for user to be ready
			Console.WriteLine("Please enter a level number (0 - ...) to (re)start LazTris:");
			Console.CursorVisible = true;
			while (!int.TryParse(Console.ReadLine(), out initlevel))
			{
				//Clear screen
				Console.Clear();
				Console.WriteLine("Please enter a number!");
			}
			Console.CursorVisible = false;
			//Clear screen
			Console.Clear();
			//Start music
			//music.PlayLooping();
			//Initialize a new game
			score = 0;
			linescompleted = 0;
			tickdelay = CalculateDelay();
			dropdelay = tickdelay/10;
			field = new PlayField(Width, Height);
			field.LostEvent += Lost;
			field.CompletedLinesEvent += CompletedLines;
			dropping = false;
			//First render
			Draw(false);
			PrintBuffer();
			update = DateTime.UtcNow.AddMilliseconds(resumedelay);
		}

		static float CalculateDelay()
		{
			//calculate tickdelay
			return 3/(linescompleted/10 + initlevel + 3.5f)*1000;
		}

		static void CompletedLines (object o, int amount)
		{
			//Great! Player completed lines!
			switch (amount)
			{
			case 1: {
					score += 40 * (linescompleted/10 + initlevel + 1);
					break;
				}
			case 2: {
					score += 100 * (linescompleted/10 + initlevel + 1);
					break;
				}
			case 3: {
					score += 300 * (linescompleted/10 + initlevel + 1);
					break;
				}
			case 4: {
					score += 1200 * (linescompleted/10 + initlevel + 1);
					break;
				}
			}
			linescompleted += amount;
		}

		static void Lost(object o, EventArgs e)
		{
			//Lost event fired: initiate a new game
			Draw(true);
			InitializeGame();
		}

		static void Draw(bool displayinformationonly)
		{
			BufferObject[] characters;
			if (!displayinformationonly)
			{
				//Playfield's buffer size includes the borders
				buffer = new BufferObject[field.Width + 2 * borderwidth, field.Height + 2 * borderwidth];

				//Loop through the buffer array
				for (int j = 0; j < buffer.GetLength(1); j++)
				{
					for (int i = 0; i < buffer.GetLength(0); i++)
					{
						//At some positions, the border has to be drawn
						if (i < borderwidth || i > buffer.GetLength(0) - borderwidth - 1 || j < borderwidth || j > buffer.GetLength(1) - borderwidth - 1)
						{
							buffer[i, j] = new BufferObject(ConsoleColor.Black, border);
						}
					}
				}

				//Add the contents of the tetris grid to the buffer
				foreach (PlayField.ColorPoint cp in field.GridContent)
				{
					//Borderwidth has to be added to the original tetris-grid coordinates
					buffer[cp.point.X + borderwidth, cp.point.Y + borderwidth] = new BufferObject(cp.Color, pixel);
				}

				//Draw the tetris-part, but only if requested
				foreach (Point p in field.Current.Points)
				{
					//Ignore coordinates that are outside the screen
					if (p.Y >= 0 && p.Y < field.Height && p.X >= 0&& p.X < field.Width)
					{
						buffer[p.X + borderwidth, p.Y + borderwidth] = new BufferObject(field.Current.Color, pixel);
					}
				}

				//Create the buffer for the display
				nextbuffer = new BufferObject[nextbufferwidth + 2 * borderwidth, nextbufferheight + 2 * borderwidth + 1];

				//Loop through the buffer array
				for (int j = 0; j < nextbuffer.GetLength(1); j++)
				{
					for (int i = 0; i < nextbuffer.GetLength(0); i++)
					{
						//At some positions, the border has to be drawn
						if (i < borderwidth || i > nextbuffer.GetLength(0) - borderwidth - 1 || j < borderwidth || j > nextbuffer.GetLength(1) - borderwidth - 1)
						{
							nextbuffer[i, j] = new BufferObject(ConsoleColor.Black, border);
						}
					}
				}

				//We want to write some text in the top center of the box
				string text = "Next:";
				characters = TextToBufferObject(text, ConsoleColor.Black);
				//Split it into double-packs and write into buffer
				for (int i = 0; i < characters.Length; i++)
				{
					nextbuffer[nextbuffer.GetLength(0)/2 - text.Length/4 + i, borderwidth] = characters[i];
				}
				//place "next" part into display
				foreach (Point p in field.Next.Points)
				{
					nextbuffer[nextbuffer.GetLength(0)/2 + p.X, nextbuffer.GetLength(1)/2 - field.Next.Height/2 + p.Y] = new BufferObject(field.Next.Color, pixel);
				}
			}
			//Create the buffer for the game data display. Size will be calculated to fit the box right into the right bottom corner
			databuffer = new BufferObject[nextbuffer.GetLength(0), buffer.GetLength(1)- nextbuffer.GetLength(1)];

			//Loop through the buffer array
			for (int j = 0; j < databuffer.GetLength(1); j++)
			{
				for (int i = 0; i < databuffer.GetLength(0); i++)
				{
					//At some positions, the border has to be drawn
					if (i < borderwidth || i > databuffer.GetLength(0) - borderwidth - 1 || j < borderwidth || j > databuffer.GetLength(1) - borderwidth - 1)
					{
						databuffer[i, j] = new BufferObject(ConsoleColor.Black, border);
					}
				}
			}

			//Write data into display
			characters = TextToBufferObject("Score:", ConsoleColor.Blue);
			for (int i = 0 ; i < characters.Length; i++)
			{
				databuffer[borderwidth + i, borderwidth] = characters[i];
			}

			characters = TextToBufferObject(score.ToString(), ConsoleColor.Black);
			for (int i = 0 ; i < characters.Length; i++)
			{
				databuffer[borderwidth + i, borderwidth + 1] = characters[i];
			}

			characters = TextToBufferObject("Level:", ConsoleColor.Blue);
			for (int i = 0 ; i < characters.Length; i++)
			{
				databuffer[borderwidth + i, borderwidth + 2] = characters[i];
			}

			characters = TextToBufferObject((linescompleted/10 + initlevel).ToString(), ConsoleColor.Black);
			for (int i = 0 ; i < characters.Length; i++)
			{
				databuffer[borderwidth + i, borderwidth + 3] = characters[i];
			}

			characters = TextToBufferObject("Lines:", ConsoleColor.Blue);
			for (int i = 0 ; i < characters.Length; i++)
			{
				databuffer[borderwidth + i, borderwidth + 4] = characters[i];
			}

			characters = TextToBufferObject(linescompleted.ToString(), ConsoleColor.Black);
			for (int i = 0 ; i < characters.Length; i++)
			{
				databuffer[borderwidth + i, borderwidth + 5] = characters[i];
			}
		}

		static BufferObject[] TextToBufferObject(string text, ConsoleColor color)
		{
			//The amount of characters has to be even
			if (text.Length % 2 != 0)
			{
				text += " ";
			}
			//One BufferObject contains two characters
			BufferObject[] result = new BufferObject[text.Length/2];
			for (int i = 0; i < text.Length/2; i++)
			{
				result[i] = new BufferObject(color, text.Substring(i * 2, 2));
			}
			return result;
		}
			
		static void PrintBuffer()
		{
			//Bring the contents of the buffer(s) to screen
			//Prepare readraw
			Console.SetCursorPosition(0,0);
			//Save default console background color
			ConsoleColor backgroundcolor = Console.BackgroundColor;

			//Loop through the buffer(s) and write the contents to screen
			//i - for = rows
			for (int i = 0; i < buffer.GetLength(1); i++)
			{
				//j - for = columns
				for (int j = 0; j < buffer.GetLength(0); j++)
				{
					Console.BackgroundColor = buffer[j, i].Color;
					Console.Write(buffer[j, i].Enabled ? buffer[j, i].Chars : playfieldbackground );
					Console.BackgroundColor = backgroundcolor;
				}

				//here we can append another buffer: the next-part-buffer
				if (i < nextbuffer.GetLength(1))
				{
					for (int j = 0; j < nextbuffer.GetLength(0); j++)
					{
						Console.BackgroundColor = nextbuffer[j, i].Color;
						Console.Write(nextbuffer[j, i].Enabled ? nextbuffer[j, i].Chars : blank );
						Console.BackgroundColor = backgroundcolor;
					}
				}
				else
				{
					//below the next-part-display
					for (int j = 0; j < databuffer.GetLength(0); j++)
					{
						Console.BackgroundColor = databuffer[j, i - nextbuffer.GetLength(1)].Color;
						Console.Write(databuffer[j, i - nextbuffer.GetLength(1)].Enabled ? databuffer[j, i - nextbuffer.GetLength(1)].Chars : blank );
						Console.BackgroundColor = backgroundcolor;
					}
				}
				Console.WriteLine();
			}
		}
	}
}
