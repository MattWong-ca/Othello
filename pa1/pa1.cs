#nullable enable
using System;
using static System.Console;

namespace Bme121
{
    record Player( string Colour, string Symbol, string Name );
    
    // The `record` is a kind of automatic class in the sense that the compiler generates
    // the fields and constructor and some other things just from this one line.
    // There's a rationale for the capital letters on the variable names (later).
    // For now you can think of it as roughly equivalent to a nonstatic `class` with three
    // public fields and a three-parameter constructor which initializes the fields.
    // It would be like the following. The 'readonly' means you can't change the field value
    // after it is set by the constructor method.
    
    //class Player
    //{
        //public readonly string Colour;
        //public readonly string Symbol;
        //public readonly string Name;
        
        //public Player( string Colour, string Symbol, string Name )
        //{
            //this.Colour = Colour;
            //this.Symbol = Symbol;
            //this.Name = Name;
        //}
    //}
    
    static partial class Program
    {
        // Display common text for the top of the screen.
        
        static void Welcome( )
        {
			WriteLine();
			WriteLine("Welcome to Othello!");
			WriteLine();
        }
        
        // Collect a player name or default to form the player record.
        
        static Player NewPlayer( string colour, string symbol, string defaultName )
        {
			Write("Type the {0} disc ({1}) player name [or <Enter> for '{2}']: ", 
				colour, symbol, defaultName);
			string name = ReadLine()!;
			if(name.Length == 0) name = defaultName;
            return new Player( colour, symbol, name );
        }
        
        // Determine which player goes first or default.
        
        static int GetFirstTurn( Player[ ] players, int defaultFirst )
        {
			while(true)
			{
				Write("Choose who will play first [or <Enter> for {0}/{1}/{2}]: ", 
					players[defaultFirst].Colour,
					players[defaultFirst].Symbol,
					players[defaultFirst].Name );
				string response = ReadLine()!;
				if(response.Length == 0) return defaultFirst;
				
				//for loop returns 0 or 1 depending on user's response 
				for (int i = 0; i < players.Length; i++)
				{
					if( players[i].Colour == response ) return i;
					if( players[i].Symbol == response ) return i;
					if( players[i].Name == response ) return i;
				}
				WriteLine( "Invalid response, please try again." );
			}
        }
        
        // Get a board size (between 4 and 26 and even) or default, for one direction.
        
        static int GetBoardSize( string direction, int defaultSize )
        {
            while (true)
            {
				Write("Enter board {0} (4 to 26, even) [or <Enter> for {1}]: ", 
					direction, defaultSize);
				string response = ReadLine();
				if( response.Length == 0 ) return defaultSize;
				int size = int.Parse(response);
				if (size >= 4 && size <= 26 && size % 2 == 0) return size;
				WriteLine("Invalid response, please try again");
			}
        }
        
        // Get a move from a player.
        
        static string GetMove( Player player )
        {
			Write($"{player.Name} make a move: ");
			string move = ReadLine();
			
            return move;
        }
        
        // Try to make a move. Return true if it worked.
        
        static bool TryMove( string[ , ] board, Player player, string move )
        {			
			const string empty = " ";

			if ( move == "skip" ) return true;
			
			if ( move.Length != 2)
			{
				WriteLine();
				Write( "The move should be two characters, ");
				Write( "one for the row and one for the column. ");
				return false;
			}
			
			int row = IndexAtLetter(move.Substring(0,1));
			if(row < 0 || row >= board.GetLength(0))
			{
				WriteLine();
				WriteLine( "The first character must be a row in the game board" );
				return false;
			}
			
			int col = IndexAtLetter(move.Substring(1,1));
			if(col < 0 || col >= board.GetLength(1))
			{
				WriteLine();
				WriteLine("The second character must be a column in the game board.");
				return false;
			}
			
			if ( board[ row, col ] != empty )
			{
				WriteLine();
				WriteLine("The cell you chose is occupied.");
				return false;
			}
			
			//call TryDirection 8 times (each direction user's move can go) 
			//to see if that direction is valid
			bool leftUp = TryDirection( board, player, row, -1, col, -1);
			bool up = TryDirection( board, player, row, -1, col, 0);
			bool rightUp = TryDirection( board, player, row, -1, col, 1);
			bool right = TryDirection( board, player, row, 0, col, 1);
			bool rightDown= TryDirection( board, player, row, 1, col, 1);
			bool down = TryDirection( board, player, row, 1, col, 0);
			bool leftDown= TryDirection( board, player, row, 1, col, -1);
			bool left = TryDirection( board, player, row, 0, col, -1);
			
			if ( leftUp || up || rightUp || right || rightDown || down || leftDown || left )
			{
				return true;
			}
			else
			{
				return false;
			}																		
        }
        
        // Do the flips along a direction specified by the row and column delta for one step.
        
        static bool TryDirection( string[ , ] board, Player player,
			int moveRow, int deltaRow, int moveCol, int deltaCol )
		{
			string empty = " ";
			 
			int nextRow = moveRow + deltaRow;
			if (nextRow < 0 || nextRow >= board.GetLength(0)) return false;
			
			int nextCol = moveCol + deltaCol;
			if (nextCol < 0 || nextCol >= board.GetLength(1)) return false;
			
			if( board[ nextRow, nextCol ] == player.Symbol || board[ nextRow, nextCol ] == empty)
		    {
				return false;
			}
			
			int count = 1;
			bool found = false;
			
			while (! found)
			{
				nextRow = nextRow + deltaRow;
				nextCol = nextCol + deltaCol;
				
				if (nextRow < 0 || nextRow >= board.GetLength(0)) return false;
				if (nextCol < 0 || nextCol >= board.GetLength(1)) return false;
				
				if (board[ nextRow, nextCol ] == empty ) return false;
				
				if ((board[nextRow,nextCol]) == player.Symbol) found = true;
				else count ++;
			}
			
			board[ moveRow, moveCol ] = player.Symbol;
			
			//flips enemy pieces to active player's pieces
			for (int i = 0; i < count; i++)
			{
				board[ moveRow + deltaRow, moveCol + deltaCol] = player.Symbol;
				moveRow = moveRow + deltaRow;
				moveCol = moveCol + deltaCol;
			}
			return true;
	
        }
        
        // Count the discs to find the score for a player.
        
        static int GetScore( string[ , ] board, Player player )
        {
			int count = 0;
			for (int i = 0; i < board.GetLength(0); i++)
			{
				for (int j = 0; j < board.GetLength(1); j++)
				{
					if ( board[i,j] == player.Symbol )
					{
						count ++;
					}
				}
			}
			return count;
        }
        
        // Display a line of scores for all players.
        
        static void DisplayScores( string[ , ] board, Player[ ] players )
        {
			WriteLine("{0}'s score is: {1}		{2}'s score is: {3}", 
				players[0].Name, GetScore(board, players[0]),
				players[1].Name, GetScore(board, players[1]));
        }
        
        // Display winner(s) and categorize their win over the defeated player(s).
        
        static void DisplayWinners( string[ , ] board, Player[ ] players )
        {
			if ( GetScore(board, players[0]) < GetScore(board, players[1]) )
			{
				WriteLine("{0} wins!", players[1].Name);
			}
			if (GetScore(board, players[0]) > GetScore(board, players[1]))
			{
				WriteLine("{0} wins!", players[0].Name);
			}
			if (GetScore(board, players[0]) == GetScore(board, players[1]))
			{
				WriteLine("It's a tie!");
			}
        }
        
        static void Main( )
        {
            // Set up the players and game.
            // Note: I used an array of 'Player' objects to hold information about the players.
            // This allowed me to just pass one 'Player' object to methods needing to use
            // the player name, colour, or symbol in 'WriteLine' messages or board operation.
            // The array aspect allowed me to use the index to keep track or whose turn it is.
            // It is also possible to use separate variables or separate arrays instead
            // of an array of objects. It is possible to put the player information in
            // global variables (static field variables of the 'Program' class) so they
            // can be accessed by any of the methods without passing them directly as arguments.
            
            Welcome( );
            
            Player[ ] players = new Player[ ] 
            {
                NewPlayer( colour: "black", symbol: "X", defaultName: "Black" ),
                NewPlayer( colour: "white", symbol: "O", defaultName: "White" ),
            };
            
            int turn = GetFirstTurn( players, defaultFirst: 0 );
           
            int rows = GetBoardSize( direction: "rows",    defaultSize: 8 );
            int cols = GetBoardSize( direction: "columns", defaultSize: 8 );
            
            string[ , ] game = NewBoard( rows, cols );
            
            // Play the game.
            
            bool gameOver = false;
            while( ! gameOver )
            {
                Welcome( );
                DisplayBoard( game ); 
                DisplayScores( game, players );
                
                string move = GetMove( players[ turn ] );
                if( move == "quit" ) gameOver = true;
                else
                {
                    bool madeMove = TryMove( game, players[ turn ], move );
                    if( madeMove ) turn = ( turn + 1 ) % players.Length;
                    else 
                    {
                        Write( " Your choice didn't work!" );
                        Write( " Press <Enter> to try again." );
                        ReadLine( ); 
                    }
                }
            }
            
            // Show fhe final results.
            
            DisplayWinners( game, players );
            WriteLine( );
        }
    }
}
