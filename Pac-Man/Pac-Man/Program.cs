using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;
using System.IO;
using System.Timers;
namespace Pac_Man
{
    class Usuario
    {
        public string nombre = null;
        public string contraseña = null;
        public int puntuacionSnake = 0;
        public int puntuacionPacman = 0;
        public int puntuacionAhorcado = 0;
        public int puntuacionSpace = 0;

        
        //public int highscore = puntuacionSnake + puntuacionPacman + puntuacionAhorcado + puntuacionSpace;

    }
    class Program
    {
        private const int MF_BYCOMMAND = 0x00000000;
        public const int SC_CLOSE = 0xF060;
        public const int SC_MINIMIZE = 0xF020;
        public const int SC_MAXIMIZE = 0xF030;
        public const int SC_SIZE = 0xF000;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [System.Runtime.InteropServices.DllImport("kernel32.dll", ExactSpelling = true)]

        private static extern IntPtr GetConsoleWindow();
       
        static string[] str = new string[80072];                    //Creamos un Array que almacene las palabras del fichero
        static Random rnd = new Random();                           //Una variable random para elegir la palabra aleatoriamente
        static int windowsX = 0, windowsY = 1;                      //La anchura y altura de la consola
        static int[,] laberinto;                                    //La matriz donde se va a guardar el mapa de Pac-Man
        static int i = 0, j = 0;                                    //Variables auxiliares para el Foreach
        static string line;                                         //Auxiliar que guarda la linea de texto del mapa
        static string direccionanterior = "E";                      //Dirección que desea el usuario, pero no pudo aplicarse
      
        static string direccion = "E";                              //Dirección actual del personaje
        static int xAux = 0, yAux = 0;                              //Variables auxiliares de la posicion del personaje
        static Queue<Point> pacPosition = new Queue<Point>();       //Guarda la posición del PacMan
        static Point posicionActual = new Point(1, 1);              //Posicion actual de pacman
        static string direccionRojo = "E";                          //Direccion del fantasma Rojo
        static string direccionPinky = "E";                         //Direccion del fantasma Pinky
        static Queue<Point> rojoPosition = new Queue<Point>();      //Guarda la posición del fantasma rojo
        static Point posicionActual_rojo = new Point(12, 12);        //Posicion actual del fantasma rojo
        static int[,] nodos;                                        //Matriz de las intersecciones del mapa
        static Queue<Point> pinkyPosition = new Queue<Point>();     //Guarda la posición del fantasma rojo
        static Point posicionActual_pinky = new Point(6, 11);       //Posicion actual del fantasma pinky
        static string fRojo = "☻";                                  //Representacion que se dibuja del fantasma rojo
        static string fPinky = "☻";                                 //Representacion que se dibuja del fantasma pinky
        static int distanciaER = 0, distanciaEP = 0;                 //Auxiliares que guardan la distancia desde el fantasma hasta el siguiente nodo
        static int distanciaSR = 0, distanciaSP = 0;
        static int distanciaOR = 0, distanciaOP = 0;
        static int distanciaNR = 0, distanciaNP = 0;
        static bool lleganodoR = true;                              //booleano para comprobar si ha llegado al nodor el fantasma Rojo
        static bool lleganodoP = true;                              //booleano para comprobar si ha llegado al nodor el fantasma Pinky
        static bool cargado = false;                                //Para generar la matriz nodos solo 1 vez
        private static System.Timers.Timer aTimerS;                 //Temporizador para controlar el tiempo de Scatter
        private static System.Timers.Timer aTimerC;                 //Temporizador para controlar el tiempo de persecución de los fantasmas
        private static System.Timers.Timer aTimerAlien;             //Temporizador para mover a los aliens
        private static System.Timers.Timer aTimerBala;              //Movimiento de la bala
        static int userIndex = 0;                                   //Indice de la lista de usuarios para la sesión actual
        static Point ultimoNodoR = new Point();                     //Ultimo nodo elegido como objetivo por el fantasma rojo
        static Point ultimoNodoP = new Point();                     //Ultimo nodo elegido como objetivo por el fantasma pinky
        static int[,] scatterRed = new int[,] { { 21, 5 }, { 21, 1 }, { 26, 1 }, { 26, 5 } };
        static int[,] scatterPinky = new int[,] { { 6, 5 }, { 6, 1 }, { 1, 1 }, { 1, 5 } };
        static int[,] dirSalida = new int[,] { { 13, 12 }, { 13, 11 }, { 13, 10 }, { 12, 10 } };
        static int scatterRedx = 0;
        static int scatterPinkyx = 0;
        static int dirSalidax = 0;
        static bool isScared = false;
        static bool isChasing = true;
        static bool isSalida = true;        
        static List<Usuario> user = new List<Usuario>();

        static void Main(string[] args)
        {
            IntPtr handle = GetConsoleWindow();
            IntPtr sysMenu = GetSystemMenu(handle, false);

            if (handle != IntPtr.Zero)
            {
                DeleteMenu(sysMenu, SC_MAXIMIZE, MF_BYCOMMAND);
                DeleteMenu(sysMenu, SC_SIZE, MF_BYCOMMAND);
            }

            Console.SetWindowSize(28, 27);
            Console.BufferWidth = 28;
            Console.BufferHeight = 28;
            Console.Title = "Menu de Juegos";


            try
            {
                using (StreamReader sr = new StreamReader("C:\\prueba\\usuarios.txt"))
                {
                    string line;

                    char c;
                    string puntacion = null;

                    do
                    {
                        int par = 0;
                        char[] parametros;
                        Usuario persona = new Usuario();
                        line = sr.ReadLine();
                        if (line != null)
                        {
                            c = '\n';
                            parametros = line.ToCharArray();
                            while (c != ' ')
                            {
                                c = parametros[par];
                                if (c != ' ')
                                    persona.nombre += c;
                                par++;
                            }
                            do
                            {
                                c = parametros[par];
                                if (c != ' ')
                                {
                                    persona.contraseña += c;
                                }
                                par++;
                            } while (c != ' ');
                            do
                            {
                                c = parametros[par];
                                if (c != ' ')
                                {
                                    puntacion += c;
                                }
                                par++;
                            } while (c != ' ');
                            persona.puntuacionSnake = int.Parse(puntacion);
                            puntacion = null;
                            do
                            {
                                c = parametros[par];
                                if (c != ' ')
                                {
                                    puntacion += c;
                                }
                                par++;
                            } while (c != ' ');
                            persona.puntuacionAhorcado = int.Parse(puntacion);
                            puntacion = null;
                            do
                            {
                                c = parametros[par];
                                if (c != ' ')
                                {
                                    puntacion += c;
                                }
                                par++;
                            } while (c != ' ');
                            persona.puntuacionPacman = int.Parse(puntacion);
                            puntacion = null;
                            do
                            {
                                c = parametros[par];
                                if (c != ' ')
                                {
                                    puntacion += c;
                                }
                                par++;
                                if (par == parametros.Length)
                                {
                                    c = ' ';
                                }
                            } while (c != ' ');
                            persona.puntuacionSpace = int.Parse(puntacion);
                            puntacion = null;
                            user.Add(persona);

                        }


                    } while (line != null);
                }
            }
            catch
            {
                using (StreamWriter sw = new StreamWriter("C:\\prueba\\usuarios.txt"))
             {
             
             }
            }


            int i = 0;
            using (StreamReader sr = new StreamReader("C:\\prueba\\palabras.txt")) //Lee el archivo palabras.txt que debe ser creado con anterioridad
            {
                for (i = 0; i < str.Length; i++)
                {
                    str[i] = sr.ReadLine();
                }
            }


            try
            {
                using (StreamReader sr = new StreamReader("C:\\prueba\\firstLevel1.txt"))
                {
                    string mLaberintoText;
                    mLaberintoText = sr.ReadLine();
                    windowsX = mLaberintoText.Length; //La anchura de la consola es igual a la longitud del texto
                    do
                    {
                        mLaberintoText = sr.ReadLine();
                        windowsY++; //Aumentamos la altura de la consola
                    } while (sr.Peek() >= 0);
                }
            }
            catch
            { }

            Console.SetBufferSize(windowsX, windowsY + 1);
            Console.SetWindowSize(windowsX, windowsY);
            MenuInicio();


        }

        //Funciones del juego Snake
        static void JuegoSnake()
        {
            Queue<Point> snake = new Queue<Point>();
            Point posicionActual = new Point(1, 1);
            snake.Enqueue(posicionActual);
            double velocidad = 60, aux = 60;
            string snakeHead = "█";
            string comida = "♥";
            int snakeLength = 8;
            int score = 0;
            int scoremax = 0;
            bool perder = false;
            Random rnd = new Random();
            Console.Title = "Snake";

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.SetWindowSize(60, 30);
            Console.BufferHeight = 30;
            Console.BufferWidth = 60;


            bool fruta = false;


            Point p = new Point();
            string direccion = "E";





            Console.CursorVisible = false;

            for (int i = 0; i < 4000; i++)
            {

                Console.BackgroundColor = ConsoleColor.Green;
                Console.Write(" ");

            }
            for (int i = 0; i < 30; i++)
            {
                Console.SetCursorPosition(59, i);
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Write(" ");
            }
            for (int i = 0; i < 59; i++)
            {
                Console.SetCursorPosition(i, 29);
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Write(" ");

            }
            for (int i = 0; i < 30; i++)
            {
                Console.SetCursorPosition(0, i);
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Write(" ");
            }
            for (int i = 0; i < 59; i++)
            {
                Console.SetCursorPosition(i, 0);
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Write(" ");

            }
            Console.SetCursorPosition(5, 0);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("Score: 0 ");

            Console.SetCursorPosition(5, 29);
            Console.Write("Nombre: {0}", user[userIndex].nombre);

            int auxIndex = -1;
            int auxmax = 0;
            scoremax = user[0].puntuacionSnake;
            foreach (Usuario u in user)
            {
                auxIndex++;
                if (u.puntuacionSnake > scoremax)
                {
                    scoremax = u.puntuacionSnake;
                    auxmax = auxIndex;
                }

            }
            Console.SetCursorPosition(31, 0);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine("Highscore: " + user[auxmax].nombre);




            Console.SetCursorPosition(55, 0);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(scoremax);







            Console.BackgroundColor = ConsoleColor.Green;





            for (; !perder;)
            {
                // Posición anterior de cabeza pasa a cola
                if (!fruta)
                {

                    p.X = rnd.Next(1, 59);
                    p.Y = rnd.Next(1, 29);

                    while (snake.Contains(p))
                    {



                        p.X = rnd.Next(1, 59);
                        p.Y = rnd.Next(1, 29);

                    }
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.SetCursorPosition(p.X, p.Y);
                    Console.Write(comida);
                    fruta = true;
                }
                Console.ForegroundColor = ConsoleColor.Black;
                Console.SetCursorPosition(posicionActual.X, posicionActual.Y);
                Console.Write(snakeHead);
                if (posicionActual.X == p.X && posicionActual.Y == p.Y)
                {
                    snakeLength++;
                    fruta = false;
                    score += 100;
                    Console.SetCursorPosition(12, 0);
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write(score);
                    Console.BackgroundColor = ConsoleColor.Green;
                }
                switch (direccion)
                {
                    case "E":
                        posicionActual.X++;

                        break;

                    case "O":
                        posicionActual.X--;

                        break;

                    case "S":
                        posicionActual.Y++;


                        break;

                    case "N":
                        posicionActual.Y--;

                        break;
                }
                if (posicionActual.X >= 59 || posicionActual.Y >= 29 || posicionActual.X <= 0 || posicionActual.Y <= 0)
                {
                    perder = true;
                }
                if (snake.Contains(posicionActual))
                {
                    perder = true;
                }



                // Dibujar nueva posición cabeza
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.SetCursorPosition(posicionActual.X, posicionActual.Y);
                Console.Write(snakeHead);
                snake.Enqueue(posicionActual);

                // Si la cola es más larga de la longitud máxima quitar la última
                if (snake.Count > snakeLength)
                {
                    Point ultimo = snake.Dequeue();
                    Console.SetCursorPosition(ultimo.X, ultimo.Y);
                    Console.Write(" ");
                }


                Thread.Sleep(Convert.ToInt32(velocidad));
                if (velocidad <= 80 && !fruta)
                {
                    velocidad = velocidad + 0.1;

                }






                if (Console.KeyAvailable)
                {
                    ConsoleKey key = Console.ReadKey(true).Key;
                    switch (key)
                    {
                        case ConsoleKey.DownArrow:
                            if ((direccion == "E") || (direccion == "O"))
                            {
                                aux = velocidad;
                                direccion = "S";
                                aux = velocidad;
                                velocidad = aux * 1.4;
                            }
                            break;
                        case ConsoleKey.LeftArrow:
                            if ((direccion == "N") || (direccion == "S"))
                            {
                                direccion = "O";

                                velocidad = aux;
                            }
                            break;
                        case ConsoleKey.RightArrow:
                            if ((direccion == "N") || (direccion == "S"))
                            {
                                direccion = "E";
                                velocidad = aux;
                            }
                            break;
                        case ConsoleKey.UpArrow:
                            if ((direccion == "E") || (direccion == "O"))
                            {
                                direccion = "N";
                                aux = velocidad;
                                velocidad = aux * 1.4;
                            }
                            break;

                    }//fin del switch (key)
                }//fin del if (Console.Available) 


            }

            if (score > user[userIndex].puntuacionSnake)
            {
                user[userIndex].puntuacionSnake = score;
                using (StreamWriter sw = new StreamWriter("C:\\prueba\\usuarios.txt"))
                {
                    foreach (Usuario u in user)
                    {
                        sw.WriteLine(u.nombre + " " + u.contraseña + " " + u.puntuacionSnake + " " + u.puntuacionAhorcado + " " + u.puntuacionPacman + " " + u.puntuacionSpace);
                    }
                }
            }
            Console.SetCursorPosition(28, 15);
            Console.Write("Game Over");
            Console.ReadKey();

            Menu();



        }
       
        //Funciones del juego Ahorcados
        static void JuegoAhorcado(int scoreG)
        {
            Console.Title = "Juego de Ahorcado";
            Console.SetWindowSize(100, 25);
            Console.SetBufferSize(100, 25);
            int index = 0;
            int score = scoreG;
            int scoremax = 0;
            int auxIndex = -1;
            int auxmax = 0;
            scoremax = user[0].puntuacionSnake;
            foreach (Usuario u in user)
            {
                auxIndex++;
                if (u.puntuacionAhorcado > scoremax)
                {
                    scoremax = u.puntuacionSnake;
                    auxmax = auxIndex;
                }

            }
            index = rnd.Next(0, str.Length); //Generamos un entero aleatorio para seleccionar una palabra del array al azar
            char[] b = new char[str[index].Length]; //Creamos un array de caracteres de longitud de la palabra seleccionada
            Console.SetCursorPosition(2, 24);
            Console.WriteLine("Highscore: {0} {1}          Sesión: {2} Score: {3}", user[auxmax].nombre, user[auxmax].puntuacionAhorcado, user[userIndex].nombre, score);
            int j = 0;
            foreach (char c in str[index]) //Almacena en el array de caracteres cada caracter de la palabra
            {

                b[j] = c;
                j++;

            }
           
            Console.SetCursorPosition(5, 0); //Creamos el campo de juego
            for (int i = 0; i < b.Length; i++)
            {
                Console.Write("_ ");


            }
            Console.WriteLine();
            
            int pos = 5;
            int intentos = 9;
            char intro;
            bool acertado = false;
            bool ganar = false;
            char[] palabra = new char[b.Length];
            int pUsada = 0;

            try
            {
                Console.SetCursorPosition(50, 1);
                Console.WriteLine("Palabras usadas");

                for (; intentos > 0 && !ganar;) //En un numero de intentos pedimos al usuario una letra
                {
                    Console.SetCursorPosition(0, 1);
                    Console.WriteLine("                                      ");
                    Console.WriteLine("                                      ");
                    Console.SetCursorPosition(0, 1);

                    Console.Write("Introduzca una letra");
                    Console.SetCursorPosition(25, 1);

                    Console.WriteLine("Número de intentos: {0}", intentos);
                    intro = Console.ReadKey().KeyChar;
                    Console.SetCursorPosition(50 + pUsada, 2);
                    Console.Write(intro);
                    pUsada += 2;
                    for (int i = 0; i < b.Length; i++) //Comparamos cada caracter del array con la letra introducida, si coincide se escribe le letra en la pantalla
                    {

                        if (intro == b[i])
                        {
                            pos = 5 + i * 2;
                            Console.SetCursorPosition(pos, 0);
                            Console.Write(b[i]); //Escribe el caracter en la pantalla
                            acertado = true;
                            palabra[i] = b[i]; //Asignamos el caracter de b al array palabra

                        }


                    }

                    if (palabra.SequenceEqual(b)) //Comparamos los dos arrays, si son iguales el usuario ha acertado la palabra
                    {
                        ganar = true;
                    }
                    if (!acertado) //Si ha acertado no disminuye el número de intentos
                        intentos--;
                    acertado = false;
                }
                Console.SetCursorPosition(0, 3);
                if (ganar)
                {
                    score += 500;
                    Console.WriteLine("Has acertado la palabra!");
                    Console.ReadKey();
                    Console.Clear();
                    JuegoAhorcado(score);
                }
                else
                {
                    Console.WriteLine("Te has ahorcado!");
                    Console.WriteLine(" ");
                    Console.WriteLine(b);
                    if (score > user[userIndex].puntuacionAhorcado)
                    {
                        user[userIndex].puntuacionAhorcado = score;
                        using (StreamWriter sw = new StreamWriter("C:\\prueba\\usuarios.txt"))
                        {
                            foreach (Usuario u in user)
                            {
                                sw.WriteLine(u.nombre + " " + u.contraseña + " " + u.puntuacionSnake + " " + u.puntuacionAhorcado + " " + u.puntuacionPacman + " " + u.puntuacionSpace);
                            }
                        }
                    }

                }
                Console.ReadKey();

            }
            catch
            {
                Console.WriteLine("No has introducido una letra");
            }
            Menu();
        }

        //Funciones del juego Pac-Man
        static void PacMan()
        {
            Console.SetWindowSize(windowsX, windowsY);
            Console.SetBufferSize(windowsX, windowsY + 1);
            posicionActual = new Point(1, 1);
            posicionActual_rojo = new Point(12, 12);
            posicionActual_pinky = new Point(12, 12);
            ConsoleKey key;

            if (!cargado)
            {
                MatrizNodos();
                cargado = true;
            }
            Console.Title = "Pac-Man";
            System.Media.SoundPlayer begin = new System.Media.SoundPlayer("C:\\prueba\\pacman_beginning.wav");
            begin.Play();


            Thread.Sleep(4000);
            System.Media.SoundPlayer eat = new System.Media.SoundPlayer("C:\\prueba\\pacman_chomp.wav");
            System.Media.SoundPlayer death = new System.Media.SoundPlayer("C:\\prueba\\pacman_death.wav");
            System.Media.SoundPlayer walk = new System.Media.SoundPlayer("C:\\prueba\\pacman_walk.wav");
            double velocidad = 160;
            string pac = "☺";

            Console.SetWindowSize(windowsX, windowsY);
            Console.BufferWidth = windowsX;
            Console.BufferHeight = windowsY + 1;
            Console.Title = "Pac-Man";




            SetTimerChase();
            int score = 0;
          

            Console.SetCursorPosition(1, 0);
            Console.BackgroundColor = ConsoleColor.Gray;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write("Score:");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Yellow;
            float time = 21;
            bool muerto = false;
            for (; !muerto;)
            {


                pacPosition.Enqueue(posicionActual);//Añadimos la posicion actual al final de la cola

                if (pacPosition.Count >= 1) //Solo queremos dibujar un pacman, por lo que al alcanzar 1 objeto o más, la cola debe eliminar la ultima posicion
                {
                    Point ultimo = pacPosition.Dequeue();
                    Console.SetCursorPosition(ultimo.X, ultimo.Y);
                    Console.Write(" ");
                }
                time++;
                if (time >= 20.25f)
                {
                    time = 0;
                    walk.Play();
                }

                ElegirDireccion(ref direccion, ref direccionanterior);
                Console.SetCursorPosition(posicionActual.X, posicionActual.Y); //Dibujamos el PacMan
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(pac);

                if (laberinto[posicionActual.X, posicionActual.Y] == 0) //Si la posicion del PacMan concuerda con una bola, aumentamos el score y transformamos la bola en vacio
                {
                    score += 10;
                    walk.Stop();
                    eat.Play();
                    time = 20.25f;
                    laberinto[posicionActual.X, posicionActual.Y] = 2;
                    Console.SetCursorPosition(10, 0);
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write(score);
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                if (laberinto[posicionActual.X, posicionActual.Y] == 3)
                {
                    score += 100;
                    walk.Stop();
                    eat.Play();
                    time = 20.25f;
                    laberinto[posicionActual.X, posicionActual.Y] = 2;
                    Console.SetCursorPosition(10, 0);
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write(score);
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    isScared = true;
                    if (aTimerC != null)
                    {
                        aTimerC.Stop();
                        aTimerC.Dispose();
                        SetTimerScatter();
                        isChasing = false;
                    }


                }


                Thread.Sleep(Convert.ToInt32(velocidad));//Velocidad del PacMan
                fantasmaRojo();
                fantasmaPinky();

                if (Console.KeyAvailable)//Comprobamos si se está pulsando una tecla
                {




                    key = Console.ReadKey(true).Key;
                    switch (key) //Si el usuario pulsa una de las flechas, entramos en caso:
                    {
                        case ConsoleKey.DownArrow:
                            posicionActual.Y++; //Si el usuario desea ir en una direccion, pero hay una pared, guarda la direccion en que se desea ir para luego pasarla a direccion

                            if (laberinto[posicionActual.X, posicionActual.Y] == 1)
                            {
                                posicionActual.Y--;
                                direccionanterior = "S";

                            }
                            else //Si hay un camino disponible, la direccion cambia de sentido
                            {
                                direccion = "S";
                                posicionActual.Y--;
                                direccionanterior = "S";
                            }
                            break;

                        case ConsoleKey.LeftArrow:

                            posicionActual.X--;

                            if (posicionActual.X < 0)
                                posicionActual.X = 0;
                            if (laberinto[posicionActual.X, posicionActual.Y] == 1)
                            {
                                posicionActual.X++;
                                direccionanterior = "O";

                            }
                            else
                            {
                                direccion = "O";
                                posicionActual.X++;
                                direccionanterior = "O";
                            }
                            break;

                        case ConsoleKey.RightArrow:

                            posicionActual.X++;
                            if (posicionActual.X == 28)
                                posicionActual.X--;

                            if (laberinto[posicionActual.X, posicionActual.Y] == 1)
                            {
                                posicionActual.X--;
                                direccionanterior = "E";

                            }
                            else
                            {
                                direccion = "E";
                                posicionActual.X--;
                                direccionanterior = "E";
                            }
                            break;

                        case ConsoleKey.UpArrow:

                            posicionActual.Y--;

                            if (laberinto[posicionActual.X, posicionActual.Y] == 1)
                            {
                                posicionActual.Y++;
                                direccionanterior = "N";


                            }
                            else
                            {
                                direccion = "N";
                                posicionActual.Y++;
                                direccionanterior = "N";
                            }
                            break;


                    }

                }





                if ((posicionActual_rojo == posicionActual || posicionActual_pinky == posicionActual) && !isScared)
                {
                    muerto = true;
                    isSalida = true;
                    dirSalidax = 0;
                }
                if (posicionActual_rojo == posicionActual && isScared)
                {
                    score += 100;
                    posicionActual_rojo = new Point(12, 12);
                    lleganodoR = true;

                }
                if (posicionActual_pinky == posicionActual && isScared)
                {
                    score += 100;
                    posicionActual_pinky = new Point(12, 12);
                    lleganodoP = true;
                }


            }

            Console.Clear();
            death.Play();
            Console.WriteLine("Te has muerto!");
            if (score > user[userIndex].puntuacionPacman)
            {
                user[userIndex].puntuacionAhorcado = score;
                using (StreamWriter sw = new StreamWriter("C:\\prueba\\usuarios.txt"))
                {
                    foreach (Usuario u in user)
                    {
                        sw.WriteLine(u.nombre + " " + u.contraseña + " " + u.puntuacionSnake + " " + u.puntuacionAhorcado + " " + u.puntuacionPacman + " " + u.puntuacionSpace);
                    }
                }
            }
            try
            {
                aTimerC.Stop();
                aTimerC.Dispose();
                aTimerS.Stop();
                aTimerS.Dispose();
            }
            catch { }
            Console.ReadKey();
            Menu();

        }
        static void CargarArchivos()
        {


            try
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Gray;
                using (StreamReader sr = new StreamReader("C:\\prueba\\firstLevel1.txt")) //Cargamos el archivo con el mapa
                {
                    string mLaberintoText;

                    line = sr.ReadLine();
                    mLaberintoText = line;

                    Console.Write(line);


                    do
                    {
                        line = sr.ReadLine();//Leemos cada linea del texto
                        mLaberintoText += line;//La almacenamos en este string

                        Console.Write(line); //Dibujamos el mapa

                    } while (sr.Peek() >= 0);




                    laberinto = new int[windowsX, windowsY]; //Creamos la matriz numérica con rango Anchura y Altura del texto
                    foreach (char c in mLaberintoText)
                    {
                        if (i == windowsX) //Lo utilizamos para la posicion de la matriz
                        {
                            j++;
                            i = 0;

                        }
                        if (c == '█') //Comprobamos si hay una pared, se le asigna a la matriz el valor 1
                        {
                            laberinto[i, j] = 1;
                        }
                        else if (c == 'o')
                        {
                            laberinto[i, j] = 3;
                        }


                        i++;
                    }

                }
                i = 0;
                j = 0;
            }
            catch (Exception e)
            {

            }
            return;
        }
        static void ElegirDireccion(ref string direccion, ref string direccionanterior)
        {
            switch (direccion) //Movemos el PacMan dependiendo de la direccion elegia
            {
                case "E":
                    xAux = posicionActual.X; //Usamos este auxiliar para guardar la posicion actual
                    posicionActual.X++; //Incrementamos la posicion actual
                    if (posicionActual.X == windowsX)
                    {
                        posicionActual.X = 0;
                    }
                    if (laberinto[posicionActual.X, posicionActual.Y] == 1) //Si en la posicion actual hay una pared, retornamos el valor antiguo de la posicion para que se pare
                    {
                        posicionActual.X = xAux;
                    }



                    if (laberinto[posicionActual.X, posicionActual.Y + 1] != 1 && direccionanterior == "S")//Si el usuario habia pulsado con anterioridad una direccion, pero había una pared, en cuanto encuentre un
                    //camino el PacMan irá en esa direccion
                    {
                        direccion = "S";
                    }
                    if (laberinto[posicionActual.X, posicionActual.Y - 1] != 1 && direccionanterior == "N")
                    {
                        direccion = "N";
                    }
                    break;

                case "O":
                    xAux = posicionActual.X;
                    posicionActual.X--;
                    if (posicionActual.X == 0)
                    {
                        posicionActual.X = windowsX - 1;
                    }
                    if (laberinto[posicionActual.X, posicionActual.Y] == 1)
                    {
                        posicionActual.X = xAux;
                    }

                    if (laberinto[posicionActual.X, posicionActual.Y + 1] != 1 && direccionanterior == "S")
                    {
                        direccion = "S";
                    }
                    if (laberinto[posicionActual.X, posicionActual.Y - 1] != 1 && direccionanterior == "N")
                    {
                        direccion = "N";
                    }
                    break;

                case "N":
                    yAux = posicionActual.Y;
                    posicionActual.Y--;
                    if (laberinto[posicionActual.X, posicionActual.Y] == 1)
                    {
                        posicionActual.Y = yAux;
                    }
                    if (laberinto[posicionActual.X + 1, posicionActual.Y] != 1 && direccionanterior == "E")
                    {
                        direccion = "E";
                    }
                    if (laberinto[posicionActual.X - 1, posicionActual.Y] != 1 && direccionanterior == "O")
                    {
                        direccion = "O";
                    }
                    break;

                case "S":
                    yAux = posicionActual.Y;
                    posicionActual.Y++;
                    if (laberinto[posicionActual.X, posicionActual.Y] == 1)
                    {
                        posicionActual.Y = yAux;
                    }
                    if (laberinto[posicionActual.X + 1, posicionActual.Y] != 1 && direccionanterior == "E")
                    {
                        direccion = "E";
                    }
                    if (laberinto[posicionActual.X - 1, posicionActual.Y] != 1 && direccionanterior == "O")
                    {
                        direccion = "O";
                    }
                    break;

            }
            return;
        }
        static void MatrizNodos()
        {
            nodos = new int[windowsX, windowsY];
            int x = 0, y = 0;


            for (y = 0; y < windowsY; y++)
            {
                for (x = 0; x < windowsX; x++)
                {
                    if ((x + 1) == windowsX || (x - 1) < 0 || (y + 1) == windowsY || laberinto[x, y] == 1)
                    {
                        nodos[x, y] = 2;
                    }
                    else
                    {
                        if (laberinto[x + 1, y] == 0)
                        {
                            if (laberinto[x, y + 1] == 0)
                            {
                                nodos[x, y] = 1;
                            }
                        }
                        if (laberinto[x - 1, y] == 0)
                        {
                            if (laberinto[x, y + 1] == 0)
                            {
                                nodos[x, y] = 1;
                            }
                        }
                        if (laberinto[x + 1, y] == 0)
                        {
                            if (laberinto[x, y - 1] == 0)
                            {
                                nodos[x, y] = 1;
                            }
                        }
                        if (laberinto[x - 1, y] == 0)
                        {
                            if (laberinto[x, y - 1] == 0)
                            {
                                nodos[x, y] = 1;
                            }
                        }
                    }

                    //Console.Write(nodos[x, y]);                                  
                }
            }
            return;
        }
        static void fantasmaRojo()
        {
            rojoPosition.Enqueue(posicionActual_rojo);
            if (rojoPosition.Count >= 1) //Solo queremos dibujar un fantasma, por lo que al alcanzar 1 objeto o más, la cola debe eliminar la ultima posicion
            {
                Point ultimo = rojoPosition.Dequeue();
                Console.SetCursorPosition(ultimo.X, ultimo.Y);
                if (laberinto[ultimo.X, ultimo.Y] == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write(".");
                }
                else
                {
                    Console.Write(" ");
                }
            }
            if (lleganodoR)
            {
                movimientoRojo();
                lleganodoR = false;
            }
            if (isSalida)
            {
                if (posicionActual_rojo == new Point(scatterRed[scatterRedx, 0], scatterRed[scatterRedx, 1]))
                {
                    scatterRedx++;
                }
            }


            MoverFantasmaRojo();

            Console.SetCursorPosition(posicionActual_rojo.X, posicionActual_rojo.Y); //Dibujamos el fantasma rojo
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(fRojo);


        }
        static void movimientoRojo()
        {
            distanciaER = 0;
            distanciaSR = 0;
            distanciaOR = 0;
            distanciaNR = 0;


            bool destino = false;
            Point posicionAntigua = posicionActual_rojo;
            while (!destino)
            {
                posicionActual_rojo.X++;
                distanciaER++;

                if (nodos[posicionActual_rojo.X, posicionActual_rojo.Y] == 1)
                {
                    destino = true;
                }
                if (nodos[posicionActual_rojo.X, posicionActual_rojo.Y] == 2)
                {
                    destino = true;
                    distanciaER = 1000;
                }
                if (posicionActual_rojo == posicionActual)
                {
                    destino = true;
                }
                if (posicionActual_rojo == ultimoNodoR)
                {
                    distanciaER = 1000;
                    destino = true;
                }

            }
            destino = false;

            posicionActual_rojo = posicionAntigua;

            while (!destino)
            {
                posicionActual_rojo.X--;
                distanciaOR++;
                if (nodos[posicionActual_rojo.X, posicionActual_rojo.Y] == 1)
                {
                    destino = true; ;
                }
                if (nodos[posicionActual_rojo.X, posicionActual_rojo.Y] == 2)
                {
                    destino = true;
                    distanciaOR = 1000;
                }
                if (posicionActual_rojo == posicionActual)
                {
                    destino = true;
                }
                if (posicionActual_rojo == ultimoNodoR)
                {
                    distanciaOR = 1000;
                    destino = true;
                }

            }
            destino = false;

            posicionActual_rojo = posicionAntigua;

            while (!destino)
            {
                posicionActual_rojo.Y++;
                distanciaSR++;
                if (nodos[posicionActual_rojo.X, posicionActual_rojo.Y] == 1)
                {
                    destino = true;
                }
                if (nodos[posicionActual_rojo.X, posicionActual_rojo.Y] == 2)
                {
                    destino = true;
                    distanciaSR = 1000;
                }
                if (posicionActual_rojo == posicionActual)
                {
                    destino = true;
                }
                if (posicionActual_rojo == ultimoNodoR)
                {
                    distanciaSR = 1000;
                    destino = true;
                }

            }
            destino = false;

            posicionActual_rojo = posicionAntigua;
            while (!destino)
            {
                posicionActual_rojo.Y--;
                distanciaNR++;
                if (nodos[posicionActual_rojo.X, posicionActual_rojo.Y] == 1)
                {
                    destino = true;
                }
                if (nodos[posicionActual_rojo.X, posicionActual_rojo.Y] == 2)
                {
                    destino = true;
                    distanciaNR = 1000;
                }
                if (posicionActual_rojo == posicionActual)
                {
                    destino = true;
                }
                if (posicionActual_rojo == ultimoNodoR)
                {
                    distanciaNR = 1000;
                    destino = true;
                }

            }
            destino = false;

            posicionActual_rojo = posicionAntigua;

            direccionRojo = ElegirCaminoRojo(distanciaER, distanciaOR, distanciaSR, distanciaNR);

        }
        static string ElegirCaminoRojo(int E, int O, int S, int N)
        {
            int dE = posicionActual_rojo.X + E;
            int dO = posicionActual_rojo.X - O;
            int dS = posicionActual_rojo.Y + S;
            int dN = posicionActual_rojo.Y - N;
            string dir = "E";

            if (scatterRedx >= 4)
            {

                scatterRedx = 0;

            }
            if (dirSalidax > 3)
            {
                isSalida = false;
            }


            if (Hipo(dE, posicionActual_rojo.Y) <= Hipo(dO, posicionActual_rojo.Y) && Hipo(dE, posicionActual_rojo.Y) <= Hipo(posicionActual_rojo.X, dS) && Hipo(dE, posicionActual_rojo.Y) <= Hipo(posicionActual_rojo.X, dN))
            {
                dir = "E";
            }
            if (Hipo(dO, posicionActual_rojo.Y) <= Hipo(dE, posicionActual_rojo.Y) && Hipo(dO, posicionActual_rojo.Y) <= Hipo(posicionActual_rojo.X, dS) && Hipo(dO, posicionActual_rojo.Y) <= Hipo(posicionActual_rojo.X, dN))
            {
                dir = "O";
            }
            if (Hipo(posicionActual_rojo.X, dS) <= Hipo(dO, posicionActual_rojo.Y) && Hipo(posicionActual_rojo.X, dS) <= Hipo(posicionActual_rojo.X, dN) && Hipo(posicionActual_rojo.X, dS) <= Hipo(dE, posicionActual_rojo.Y))
            {
                dir = "S";
            }
            if (Hipo(posicionActual_rojo.X, dN) <= Hipo(dO, posicionActual_rojo.Y) && Hipo(posicionActual_rojo.X, dN) <= Hipo(posicionActual_rojo.X, dS) && Hipo(posicionActual_rojo.X, dN) <= Hipo(dE, posicionActual_rojo.Y))
            {
                dir = "N";
            }

            ultimoNodoR = posicionActual_rojo;

            return dir;
        }
        static void MoverFantasmaRojo()
        {
            switch (direccionRojo) //Movemos el fantasma dependiendo de la direccion elegia
            {
                case "E":
                    if (distanciaER > 0)
                    {
                        posicionActual_rojo.X++;
                        distanciaER--;
                    }
                    else
                    {
                        lleganodoR = true;
                    }
                    break;

                case "O":

                    if (distanciaOR > 0)
                    {
                        posicionActual_rojo.X--;
                        distanciaOR--;
                    }
                    else
                    {
                        lleganodoR = true;
                    }
                    break;

                case "N":

                    if (distanciaNR > 0)
                    {
                        posicionActual_rojo.Y--;
                        distanciaNR--;
                    }
                    else
                    {
                        lleganodoR = true;
                    }

                    break;

                case "S":

                    if (distanciaSR > 0)
                    {
                        posicionActual_rojo.Y++;
                        distanciaSR--;
                    }
                    else
                    {
                        lleganodoR = true;
                    }

                    break;

            }
            return;
        }
        static double Hipo(int x, int y)
        {
            double hipotenusa = 0.0f;
            int auX = 0, auY = 0;

            if (isChasing)
            {
                auX = posicionActual.X;
                auY = posicionActual.Y;
            }
            else
            {
                auX = scatterRed[scatterRedx, 0];
                auY = scatterRed[scatterRedx, 1];

            }
            if (isSalida)
            {
                auX = dirSalida[dirSalidax, 0];
                auY = dirSalida[dirSalidax, 1];
            }

            hipotenusa = (Math.Sqrt((auX - x) * (auX - x) + (auY - y) * (auY - y)));

            return hipotenusa;
        }
        static double HipoPinky(int x, int y)
        {
            double hipotenusa = 0.0f;
            int auX = posicionActual.X, auY = posicionActual.Y;
            bool destino = false;
            if (direccion == "E")
            {
                do
                {
                    auX++;
                    if (nodos[auX, auY] == 1)
                    {
                        destino = true;
                    }
                    if (nodos[auX, auY] == 2)
                    {
                        destino = true;
                        auX = posicionActual.X;
                    }
                } while (!destino);
            }
            if (direccion == "O")
            {
                do
                {
                    auX--;
                    if (nodos[auX, auY] == 1)
                    {
                        destino = true;
                    }
                    if (nodos[auX, auY] == 2)
                    {
                        destino = true;
                        auX = posicionActual.X;
                    }
                } while (!destino);
            }
            if (direccion == "S")
            {
                do
                {
                    auY++;
                    if (nodos[auX, auY] == 1)
                    {
                        destino = true;
                    }
                    if (nodos[auX, auY] == 2)
                    {
                        destino = true;
                        auY = posicionActual.Y;
                    }
                } while (!destino);
            }
            if (direccion == "S")
            {
                do
                {
                    auY--;
                    if (nodos[auX, auY] == 1)
                    {
                        destino = true;
                    }
                    if (nodos[auX, auY] == 2)
                    {
                        destino = true;
                        auY = posicionActual.Y;
                    }
                } while (!destino);
            }
            if (isChasing)
            {
                auX = posicionActual.X;
                auY = posicionActual.Y;
            }
            else if (!isChasing)
            {
                auX = scatterPinky[scatterPinkyx, 0];
                auY = scatterPinky[scatterPinkyx, 1];



            }
            if (isSalida)
            {
                auX = dirSalida[dirSalidax, 0];
                auY = dirSalida[dirSalidax, 1];
            }


            hipotenusa = (Math.Sqrt((auX - x) * (auX - x) + (auY - y) * (auY - y)));

            return hipotenusa;
        }
        static void fantasmaPinky()
        {
            pinkyPosition.Enqueue(posicionActual_pinky);
            if (pinkyPosition.Count >= 1) //Solo queremos dibujar un fantasma, por lo que al alcanzar 1 objeto o más, la cola debe eliminar la ultima posicion
            {
                Point ultimo = pinkyPosition.Dequeue();
                Console.SetCursorPosition(ultimo.X, ultimo.Y);
                if (laberinto[ultimo.X, ultimo.Y] == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write(".");
                }
                else
                {
                    Console.Write(" ");
                }
            }
            if (lleganodoP)
            {
                movimientoPinky();
                lleganodoP = false;
            }
            if (posicionActual_pinky == new Point(scatterPinky[scatterPinkyx, 0], scatterRed[scatterPinkyx, 1]))
            {
                scatterPinkyx++;
            }
            if (isSalida)
            {
                if (posicionActual_pinky == new Point(dirSalida[dirSalidax, 0], dirSalida[dirSalidax, 1]))
                {
                    dirSalidax++;
                }
            }

            MoverFantasmaPinky();

            Console.SetCursorPosition(posicionActual_pinky.X, posicionActual_pinky.Y); //Dibujamos el fantasma rojo
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write(fPinky);
        }
        static void movimientoPinky()
        {
            distanciaEP = 0;
            distanciaSP = 0;
            distanciaOP = 0;
            distanciaNP = 0;


            bool destino = false;
            Point posicionAntigua = posicionActual_pinky;
            while (!destino)
            {
                posicionActual_pinky.X++;
                distanciaEP++;

                if (nodos[posicionActual_pinky.X, posicionActual_pinky.Y] == 1)
                {
                    destino = true;
                }
                if (nodos[posicionActual_pinky.X, posicionActual_pinky.Y] == 2)
                {
                    destino = true;
                    distanciaEP = 1000;
                }
                if (posicionActual_pinky == posicionActual)
                {
                    destino = true;
                }
                if (posicionActual_pinky == ultimoNodoP)
                {
                    distanciaEP = 1000;
                    destino = true;
                }

            }
            destino = false;

            posicionActual_pinky = posicionAntigua;

            while (!destino)
            {
                posicionActual_pinky.X--;
                distanciaOP++;
                if (nodos[posicionActual_pinky.X, posicionActual_pinky.Y] == 1)
                {
                    destino = true; ;
                }
                if (nodos[posicionActual_pinky.X, posicionActual_pinky.Y] == 2)
                {
                    destino = true;
                    distanciaOP = 1000;
                }
                if (posicionActual_pinky == posicionActual)
                {
                    destino = true;
                }
                if (posicionActual_pinky == ultimoNodoP)
                {
                    distanciaOP = 1000;
                    destino = true;
                }

            }
            destino = false;

            posicionActual_pinky = posicionAntigua;

            while (!destino)
            {
                posicionActual_pinky.Y++;
                distanciaSP++;
                if (nodos[posicionActual_pinky.X, posicionActual_pinky.Y] == 1)
                {
                    destino = true;
                }
                if (nodos[posicionActual_pinky.X, posicionActual_pinky.Y] == 2)
                {
                    destino = true;
                    distanciaSP = 1000;
                }
                if (posicionActual_pinky == posicionActual)
                {
                    destino = true;
                }
                if (posicionActual_pinky == ultimoNodoP)
                {
                    distanciaSP = 1000;
                    destino = true;
                }

            }
            destino = false;

            posicionActual_pinky = posicionAntigua;
            while (!destino)
            {
                posicionActual_pinky.Y--;
                distanciaNP++;
                if (nodos[posicionActual_pinky.X, posicionActual_pinky.Y] == 1)
                {
                    destino = true;
                }
                if (nodos[posicionActual_pinky.X, posicionActual_pinky.Y] == 2)
                {
                    destino = true;
                    distanciaNP = 1000;
                }
                if (posicionActual_pinky == posicionActual)
                {
                    destino = true;
                }
                if (posicionActual_pinky == ultimoNodoP)
                {
                    distanciaNP = 1000;
                    destino = true;
                }

            }
            destino = false;

            posicionActual_pinky = posicionAntigua;

            direccionPinky = ElegirCaminoPinky(distanciaEP, distanciaOP, distanciaSP, distanciaNP);
        }
        static string ElegirCaminoPinky(int E, int O, int S, int N)
        {
            int dE = posicionActual_pinky.X + E;
            int dO = posicionActual_pinky.X - O;
            int dS = posicionActual_pinky.Y + S;
            int dN = posicionActual_pinky.Y - N;
            string dir = "E";


            if (scatterPinkyx >= 4)
            {

                scatterPinkyx = 0;

            }
            if (dirSalidax > 3)
            {
                isSalida = false;
            }

            if (HipoPinky(dE, posicionActual_pinky.Y) <= HipoPinky(dO, posicionActual_pinky.Y) && HipoPinky(dE, posicionActual_pinky.Y) <= HipoPinky(posicionActual_pinky.X, dS) && HipoPinky(dE, posicionActual_pinky.Y) <= HipoPinky(posicionActual_pinky.X, dN))
            {
                dir = "E";
            }
            if (HipoPinky(dO, posicionActual_pinky.Y) <= HipoPinky(dE, posicionActual_pinky.Y) && HipoPinky(dO, posicionActual_pinky.Y) <= HipoPinky(posicionActual_pinky.X, dS) && HipoPinky(dO, posicionActual_pinky.Y) <= HipoPinky(posicionActual_pinky.X, dN))
            {
                dir = "O";
            }
            if (HipoPinky(posicionActual_pinky.X, dS) <= HipoPinky(dO, posicionActual_pinky.Y) && HipoPinky(posicionActual_pinky.X, dS) <= HipoPinky(posicionActual_pinky.X, dN) && HipoPinky(posicionActual_pinky.X, dS) <= HipoPinky(dE, posicionActual_pinky.Y))
            {
                dir = "S";
            }
            if (HipoPinky(posicionActual_pinky.X, dN) <= HipoPinky(dO, posicionActual_pinky.Y) && HipoPinky(posicionActual_pinky.X, dN) <= HipoPinky(posicionActual_pinky.X, dS) && HipoPinky(posicionActual_pinky.X, dN) <= HipoPinky(dE, posicionActual_pinky.Y))
            {
                dir = "N";
            }
            ultimoNodoP = posicionActual_pinky;

            return dir;
        }
        static void MoverFantasmaPinky()
        {
            switch (direccionPinky) //Movemos el fantasma dependiendo de la direccion elegia
            {
                case "E":
                    if (distanciaEP > 0)
                    {
                        posicionActual_pinky.X++;
                        distanciaEP--;
                    }
                    else
                    {
                        lleganodoP = true;
                    }
                    break;

                case "O":

                    if (distanciaOP > 0)
                    {
                        posicionActual_pinky.X--;
                        distanciaOP--;
                    }
                    else
                    {
                        lleganodoP = true;
                    }
                    break;

                case "N":

                    if (distanciaNP > 0)
                    {
                        posicionActual_pinky.Y--;
                        distanciaNP--;
                    }
                    else
                    {
                        lleganodoP = true;
                    }

                    break;

                case "S":

                    if (distanciaSP > 0)
                    {
                        posicionActual_pinky.Y++;
                        distanciaSP--;
                    }
                    else
                    {
                        lleganodoP = true;
                    }

                    break;

            }
            return;

        }
        private static void SetTimerScatter()
        {
            // Create a timer with a two second interval.
            aTimerS = new System.Timers.Timer(7000);
            // Hook up the Elapsed event for the timer. 
            aTimerS.Elapsed += OnTimedEvent;
            aTimerS.AutoReset = true;
            aTimerS.Enabled = true;
        }
        private static void SetTimerChase()
        {
            // Create a timer with a two second interval.
            aTimerC = new System.Timers.Timer(20000);
            // Hook up the Elapsed event for the timer. 
            aTimerC.Elapsed += OnEvent;
            aTimerC.AutoReset = true;
            aTimerC.Enabled = true;
        }
        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            SetTimerChase();
            aTimerS.Stop();
            aTimerS.Dispose();
            isChasing = true;
            isScared = false;

        }
        private static void OnEvent(Object source, ElapsedEventArgs e)
        {
            aTimerC.Stop();
            aTimerC.Dispose();
            SetTimerScatter();
            isChasing = false;
        }


        //Funciones del juego Space Invader
        static Point posicionNave = new Point(10, 40);
        static List<Point> alienPosition = new List<Point>();
        static List<Point> balaPosition = new List<Point>();
        static string alienImage = "☼";
        static string balaNave = "|";
        static bool isMoving = false;
        static int scoreInvader = 0;
        static int scoremaxInvader = 0;
        static int auxIndexSpace = 0;
        static int alienCount = 0;
        static void SpaceInvader()
        {

            Console.CursorVisible = false;
            string naveImage = "♠";

            ConsoleKey key;
            scoreInvader = 0;
            scoremaxInvader = 0;
            Console.SetBufferSize(100, 50);
            Console.SetWindowSize(100, 50);
            Console.SetCursorPosition(10, 2);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.SetCursorPosition(posicionNave.X, posicionNave.Y);
            Console.Write(naveImage);
            alienCount = 0;
            for (int y = 2; y <= 8; y += 2)
            {
                for (int x = 10; x <= 90; x += 10)
                {
                    Console.SetCursorPosition(x, y);
                    Console.Write(alienImage);
                    alienPosition.Add(new Point(x, y));
                    alienCount++;
                }
            }
            SetTimerBala();
            SetTimerAlien();
            int index = 0;
            
            foreach (Usuario u in user)
            {
                if (u.puntuacionSpace > scoremaxInvader)
                {
                    scoremaxInvader = u.puntuacionSpace;
                    auxIndexSpace = index;
                }
                index++;
            }
           
            for (;;)
            {


                if (Console.KeyAvailable && !isMoving && posicionNave.X > 0 && posicionNave.X < 99)
                {

                    key = Console.ReadKey(true).Key;
                    for (int i = 0; i < 100; i++)
                    {
                        Console.SetCursorPosition(i, 40);
                        Console.Write(" ");

                    }
                    switch (key)
                    {
                        case ConsoleKey.LeftArrow:
                            posicionNave.X--;
                            break;
                        case ConsoleKey.RightArrow:
                            posicionNave.X++;
                            break;
                        case ConsoleKey.Spacebar:
                            DisparoNave();
                            break;

                    }

                    Console.SetCursorPosition(posicionNave.X, posicionNave.Y);
                    Console.Write(naveImage + "\b");

                }
                if (posicionNave.X > 0)
                { }
                else
                {
                    posicionNave.X++;
                }
                if (posicionNave.X < 99)
                { }
                else
                {
                    posicionNave.X--;
                }

                isMoving = false;


            }



        }
        private static void SetTimerAlien()
        {
            // Create a timer with a two second interval.
            aTimerAlien = new System.Timers.Timer(3000);
            // Hook up the Elapsed event for the timer. 
            aTimerAlien.Elapsed += OnAlienTime;
            aTimerAlien.AutoReset = true;
            aTimerAlien.Enabled = true;
        }
        private static void OnAlienTime(Object source, ElapsedEventArgs e)
        {
            aTimerAlien.Stop();
            aTimerAlien.Dispose();
            SetTimerAlien();
            MoverAlien();


        }
        private static void SetTimerBala()
        {
            // Create a timer with a two second interval.
            aTimerBala = new System.Timers.Timer(100);

            // Hook up the Elapsed event for the timer. 
            aTimerBala.Elapsed += OnBalaTime;
            aTimerBala.AutoReset = true;
            aTimerBala.Enabled = true;

        }
        private static void OnBalaTime(Object source, ElapsedEventArgs e)
        {
            aTimerBala.Stop();
            aTimerBala.Dispose();
            SetTimerBala();
            isMoving = true;
            bool posEncontrada = false;

            for (int y = 0; y < balaPosition.Count && !posEncontrada; y++)
            {
                Console.SetCursorPosition(balaPosition[y].X, balaPosition[y].Y);
                Console.Write(" \b");
                if (balaPosition[y].Y - 1 < 1)
                {
                    balaPosition.Remove(balaPosition[y]);
                }
                else
                {
                    balaPosition[y] = new Point(balaPosition[y].X, balaPosition[y].Y - 1);
                    Console.SetCursorPosition(balaPosition[y].X, balaPosition[y].Y);
                    Console.Write(balaNave + "\b");
                }
                for (int x = 0; x < alienPosition.Count && !posEncontrada; x++)
                {
                    if (y >= balaPosition.Count)
                    {
                        posEncontrada = true;
                    }
                    else
                    {
                        if (alienPosition[x] == balaPosition[y])
                        {
                            Console.SetCursorPosition(balaPosition[y].X, balaPosition[y].Y);
                            Console.Write(" \b");

                            alienPosition.Remove(alienPosition[x]);
                            balaPosition.Remove(balaPosition[y]);
                            alienCount--;
                            scoreInvader += 10;
                            Console.SetCursorPosition(1, 0);
                            Console.Write("Sesion: {0} Score: {1}               Highscore: {2}  {3}", user[userIndex].nombre, scoreInvader, user[auxIndexSpace].nombre, scoremaxInvader);
                            posEncontrada = true;
                            if(alienCount<=0)
                            {
                                Console.SetCursorPosition(25, 5);
                                Console.WriteLine("Has ganado!");
                                Console.ReadKey();
                                if (scoreInvader > user[userIndex].puntuacionSpace)
                                {
                                    user[userIndex].puntuacionAhorcado = scoreInvader;
                                    using (StreamWriter sw = new StreamWriter("C:\\prueba\\usuarios.txt"))
                                    {
                                        foreach (Usuario u in user)
                                        {
                                            sw.WriteLine(u.nombre + " " + u.contraseña + " " + u.puntuacionSnake + " " + u.puntuacionAhorcado + " " + u.puntuacionPacman + " " + u.puntuacionSpace);
                                        }
                                    }
                                }
                                Menu();
                            }
                        }
                    }

                }
            }
        }
        static void MoverAlien()
        {

            for (int i = 0; i < alienPosition.Count; i++)
            {
                Console.SetCursorPosition(alienPosition[i].X, alienPosition[i].Y);
                Console.Write(" ");

                alienPosition[i] = new Point(alienPosition[i].X, alienPosition[i].Y + 1);
                Console.SetCursorPosition(alienPosition[i].X, alienPosition[i].Y);
                Console.Write(alienImage);

            }

        }
        static void DisparoNave()
        {

            Point posicionBala = new Point(posicionNave.X, posicionNave.Y - 1);
            balaPosition.Add(posicionBala);

        }

        //Funciones del menu
        static void Menu()
        {
            int selMenu = 0;
            Console.CursorVisible = false;
            Console.Clear();
            Console.ResetColor();
            do
            {
                Console.Clear();
                Console.WriteLine("1.Juego de Snake.");
                Console.WriteLine("2.Juego de Ahorcado.");
                Console.WriteLine("3.Juego de Pac-Man.");
                Console.WriteLine("4.Juego de Space-Invader.");
                Console.WriteLine("5.HighScore.");

                selMenu = (int)Char.GetNumericValue(Console.ReadKey().KeyChar);
            } while (selMenu != 1 && selMenu != 2 && selMenu != 3 && selMenu != 4 && selMenu != 5);

            switch (selMenu)
            {
                case 1:
                    {
                        Console.Clear();
                        Console.ResetColor();
                        JuegoSnake();

                    }
                    break;
                case 2:
                    Console.Clear();
                    Console.ResetColor();
                    JuegoAhorcado(0);
                    break;
                case 3:
                    Console.Clear();
                    Console.ResetColor();
                    CargarArchivos();
                    PacMan();
                    break;
                case 4:
                    Console.Clear();
                    Console.ResetColor();
                    SpaceInvader();
                    break;
                case 5:
                    Console.Clear();
                    Console.ResetColor();
                    HighScore();
                    break;
            }
            selMenu = 0;
        }
        static void RegistrarSesion()
        {

            string contra = null;
            bool isExistente = false;
            ConsoleKeyInfo key;
            Usuario usuario = new Usuario();
            do
            {
                isExistente = false;
                Console.Clear();
                Console.Write("Usuario: \nContraseña:");
                Console.SetCursorPosition(8, 0);
                usuario.nombre = Console.ReadLine();

                foreach (Usuario u in user)
                {

                    if (u.nombre == usuario.nombre)
                    {
                        isExistente = true;
                        Console.SetCursorPosition(0, 2);
                        Console.WriteLine("El nombre ya existe.");
                        Console.ReadKey();

                    }

                }

            } while (isExistente);


            Console.SetCursorPosition(11, 1);
            do
            {
                key = Console.ReadKey(true);

                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    usuario.contraseña += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && usuario.contraseña.Length > 0)
                    {
                        usuario.contraseña = usuario.contraseña.Substring(0, (usuario.contraseña.Length - 1));
                        Console.Write("\b \b");
                    }
                }
            } while (key.Key != ConsoleKey.Enter);


            user.Add(usuario);
            using (StreamWriter sw = new StreamWriter("C:\\prueba\\usuarios.txt"))
            {
                foreach (Usuario u in user)
                {
                    sw.WriteLine(u.nombre + " " + u.contraseña + " " + u.puntuacionSnake + " " + u.puntuacionAhorcado + " " + u.puntuacionPacman + " " + u.puntuacionSpace);
                }
            }
            MenuInicio();


        }
        static void IniciarSesion()
        {
            Usuario usuario = new Usuario();
            Console.Clear();
            Console.Write("Usuario: \nContraseña: ");
            Console.SetCursorPosition(8, 0);
            usuario.nombre = Console.ReadLine();

            ConsoleKeyInfo key;
            bool isUser = false;

            for (int i = 0; i < user.Count && !isUser; i++)
            {
                if (user[i].nombre == usuario.nombre)
                {
                    userIndex = i;
                    isUser = true;
                    do
                    {


                        Console.SetCursorPosition(11, 1);
                        Console.Write("              ");
                        Console.SetCursorPosition(11, 1);
                        do
                        {
                            key = Console.ReadKey(true);

                            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                            {
                                usuario.contraseña += key.KeyChar;
                                Console.Write("*");
                            }
                            else
                            {
                                if (key.Key == ConsoleKey.Backspace && usuario.contraseña.Length > 0)
                                {
                                    usuario.contraseña = usuario.contraseña.Substring(0, (usuario.contraseña.Length - 1));
                                    Console.Write("\b \b");
                                }
                            }
                        } while (key.Key != ConsoleKey.Enter);

                        if (usuario.contraseña == user[userIndex].contraseña)
                        {
                            Menu();
                        }
                        else
                        {
                            usuario.contraseña = null;
                            Console.SetCursorPosition(0, 2);
                            Console.Write("Contraseña incorrecta!");

                            Console.ReadKey();
                            Console.SetCursorPosition(0, 2);
                            Console.Write("                         ");

                        }
                    } while (usuario.contraseña != user[i].contraseña);

                }
            }
            if (!isUser)
            {
                Console.Clear();
                Console.WriteLine("El usuario que has introducido no existe.");
                Console.ReadKey();
                MenuInicio();
            }



        }
        static void MenuInicio()
        {
            Console.Clear();
            int opmenu = 0;
            do
            {
                Console.WriteLine("1.Inicio de Sesión.");
                Console.WriteLine("2.Registro de Sesión.");


                opmenu = (int)Char.GetNumericValue(Console.ReadKey().KeyChar);
            } while (opmenu != 1 && opmenu != 2 && opmenu != 3);

            switch (opmenu)
            {
                case 1:
                    {
                        Console.Clear();

                        IniciarSesion();

                    }
                    break;
                case 2:
                    Console.Clear();
                    RegistrarSesion();
                    break;

            }

        }
        static void HighScore()
        {
            Console.SetWindowSize(75, 30);
            Console.SetBufferSize(75, 30);
            Console.SetWindowSize(75, 30);
            int selMenu = 0;

            Console.Clear();
            Console.ResetColor();


            do
            {
                Console.WriteLine("1.Top jugadores.");
                Console.WriteLine("2.Top puntación Snake.");
                Console.WriteLine("3.Top puntación Ahorcado.");
                Console.WriteLine("4.Top puntación Pac-Man.");
                Console.WriteLine("5.Top puntación SpaceInvasion.");

                selMenu = (int)Char.GetNumericValue(Console.ReadKey().KeyChar);
            } while (selMenu != 1 && selMenu != 2 && selMenu != 3 && selMenu != 4 && selMenu != 5);

            switch (selMenu)
            {
                case 1:
                    {
                        Console.Clear();
                        OrdenarPuntuacionMaxima();

                        Console.ReadKey();
                        Menu();
                    }
                    break;
                case 2:
                    {
                        Console.Clear();
                    }
                    break;
                case 3:
                    {
                        Console.Clear();
                    }
                    break;
                case 4:
                    {
                        Console.Clear();
                    }
                    break;
            }
            selMenu = 0;


        }
        static void OrdenarPuntuacionMaxima()
        {
            int auxMax = 0;
            List<int> ordenados = new List<int>();
            foreach (Usuario u in user)
            {
                ordenados.Add(u.puntuacionAhorcado + u.puntuacionPacman + u.puntuacionSpace + u.puntuacionSnake);
               
            }


            ordenados.Sort((a, b) => -1 * a.CompareTo(b));
            bool isScore = false;
            int auxY = 3;
            int i = -1;
            int auxOrd = 1;

            List<string> encontrados = new List<string>();
            foreach (int punt in ordenados)
            {
                do
                {
                    i++;
                    auxMax = user[i].puntuacionAhorcado + user[i].puntuacionSpace + user[i].puntuacionPacman + user[i].puntuacionSnake;
                    if (punt == auxMax && !encontrados.Contains(user[i].nombre))
                    {
                        isScore = true;
                        encontrados.Add(user[i].nombre);
                    }
                } while (i < user.Count && !isScore);
                Console.SetCursorPosition(5, auxY);
                Console.Write(auxOrd + "." + user[i].nombre);
                for (int j = user[i].nombre.Length; j < 50; j++)
                {
                    Console.Write(".");
                }
                Console.Write(punt);
                isScore = false;
                auxY++;
                i = -1;
                auxOrd++;
            }
            return;

        }
    }
    }
        
       
    

