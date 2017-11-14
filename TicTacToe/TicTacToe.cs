//========================================================================================
// TicTacToe by Daan Juttmann
// Created: 2017-11-12
// License: GNU General Public License 3.0 (https://www.gnu.org/licenses/gpl-3.0.en.html).
﻿//========================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace TicTacToe
{
  enum Player
  {
    Player1,
    Player2
  }

  enum Status
  {
    Unresolved,
    Win,
    Loss,
    Draw
  }

﻿//========================================================================================
// Struct GameEvaluation
﻿//========================================================================================

  struct GameEvaluation
  {
    public Status status;
    public int row;
    public int col;
    public int marker;
    public int distance;

    // Constructor
    public GameEvaluation (Status evaluation)
    {
      status = evaluation;
      row = -1;
      col = -1;
      marker = 0;
      distance = -1;
    }
  }

﻿//========================================================================================
// Class TicTacToeBoard
﻿//========================================================================================

  class TicTacToeBoard
  {
    private int [,] board;
    public Player ActivePlayer {get; private set;}


    // Constructor.
    public TicTacToeBoard ()
    {
      board = new int [3, 3];
      Clear ();
    }


    // Reset the board for a new game.
    public void Clear ()
    {
      for (int i = 0; i < 3; i++)
        for (int j = 0; j < 3; j++)
          board [i, j] = 0;
      ActivePlayer = Player.Player1;
    }


    // Get a unique integer ID for the position on the board.
    public int GetID ()
    {
      int id = 0;
      for (int i = 0; i < 3; i++)
        for (int j = 0; j < 3; j++)
        {
          id *= 3;
          id += board [i, j];
        }
      return id;
    }

    
    // Copy the boards contents from another board.
    public void CopyFrom (TicTacToeBoard source)
    {
      for (int i = 0; i < 3; i++)
        for (int j = 0; j < 3; j++)
          board [i, j] = source.board [i, j];
      ActivePlayer = source.ActivePlayer;
    }


    // Make a move by placing marker at (row, col), returns true on success.
    public bool MakeMove (int row, int col, int marker)
    {
      if (GetField (row, col) != 0 || marker < 1 || marker > 2)
        return false;
      board [row, col] = marker;
      switch (ActivePlayer)
      {
      case Player.Player1:
        ActivePlayer = Player.Player2;
        break;
      case Player.Player2:
        ActivePlayer = Player.Player1;
        break;
      default:
        break;
      }
      return true;
    }


    // Returns state of a field on the board, or -1 for invalid coordinates.
    public int GetField (int row, int col)
    {
      if (row >= 0 && row < 3 && col >= 0 && col < 3)
        return board [row, col];
      return -1;
    }


    // Checks if all fields of the board are filled in.
    public bool IsFilled ()
    {
      for (int i = 0; i < 3; i++)
        for (int j = 0; j < 3; j++)
          if (board [i, j] == 0)
            return false;
      return true;
    }


    // Checks if a 3 in a row has been made on the board at coordinates
    // (startRow, startCol), going in the direction (diffRow, diffCol).
    public bool Has3InARow (int startRow, int startCol, int diffRow, int diffCol)
    {
      int marker = board [startRow, startCol];
      if (marker == 0)
        return false; // not a 3 in a row if the starting square is empty
      for (int i = 0; i < 2; i++)
      {
        startRow += diffRow;
        startCol += diffCol;
        if (board [startRow, startCol] != marker)
          return false;
      }
      return true;
    }


    // Checks if there is any 3 in a row on the board.
    public bool Has3InARow ()
    {
      for (int i = 0; i < 3; i++) // check rows
      {
        if (Has3InARow (i, 0, 0, 1))
          return true;
      }
      for (int j = 0; j < 3; j++) // check columns
      {
        if (Has3InARow (0, j, 1, 0))
          return true;
      }
      if (Has3InARow (0, 0, 1, 1)) // check diagonal
        return true;
      if (Has3InARow (2, 0, -1, 1)) // check antidiagonal
        return true;
      return false;
    }
  }

﻿//========================================================================================
// Class TicTacToeGame
﻿//========================================================================================

  class TicTacToeGame
  {
    private const int stateSpaceSize = 19683; // = 3^9, Upper limit on # board positions
    public delegate Status StatusChecker (TicTacToeBoard board);

    private TicTacToeBoard gameBoard;
    private GameEvaluation [] StateSpace; // Saves for each position whether it is winning/losing/drawn
    StatusChecker CheckGameStatus; // check whether game is won, lost, drawn or ongoing
    private int [] Player1Markers; // markers that player 1 is allowed to place
    private int [] Player2Markers; //          "          2         "

    private TicTacToeBoard [] GameAnalysis; // game positions used by AI.

    public Player ActivePlayer // which player moves next
    {
      get {return gameBoard.ActivePlayer;} 
    }
    public Status Status // status of the game for active player (unresolved, win, loss, draw)
    {
      get {return CheckGameStatus (gameBoard);}
    }
    public int [] ValidMarkers // Which markers may be used by current player.
    {
      get {return ActivePlayer == Player.Player1 ? Player1Markers : Player2Markers;}
    }


    // Constructor
    public TicTacToeGame ()
    {
      gameBoard = new TicTacToeBoard ();
      StateSpace = new GameEvaluation [stateSpaceSize];
      ClearStateSpace ();
      GameAnalysis = new TicTacToeBoard [10];
      for (int i = 0; i < 10; i++)
        GameAnalysis [i] = new TicTacToeBoard ();
    }


    // Clear all saved data from the state space.
    private void ClearStateSpace ()
    {
      for (int i = 0; i < stateSpaceSize; i++)
        StateSpace [i] = new GameEvaluation (Status.Unresolved);
    }


    // Set up a new game with method S to check winning conditions, and arrays of
    // markers that each player may use.
    public void NewGame (StatusChecker S, int [] Player1Markers, int [] Player2Markers)
    {
      gameBoard.Clear ();
      CheckGameStatus = null;
      CheckGameStatus = S;
      this.Player1Markers = Player1Markers;
      this.Player2Markers = Player2Markers;
      ClearStateSpace ();
    }


    // Returns state of a field on the board, or -1 for invalid coordinates.
    public int GetField (int row, int col)
    {
      return gameBoard.GetField (row, col);
    }


    // Try to make a move by placing marker at (row, col), return false on failure.
    public bool MakeMove (int row, int col, int marker)
    {
      if (ValidMarkers.Contains (marker) && Status == Status.Unresolved)
        return gameBoard.MakeMove (row, col, marker);
      if (!ValidMarkers.Contains (marker))
        MessageBox.Show ("Invalid marker!");
      return false;
    }


    // Make move using computer AI, returns false if game has ended.
    public bool MakeComputerMove (ref int row, ref int col, ref int marker)
    {
      if (!FindBestMove (ref row, ref col, ref marker))
        return false;
      return gameBoard.MakeMove (row, col, marker);
    }


    // Check if a field is empty.
    public bool FieldIsEmpty (int row, int col)
    {
      return gameBoard.GetField (row, col) == 0;
    }


    // Check if a marker may be used by the current player
    public bool MarkerIsValid (int marker)
    {
      if (ActivePlayer == Player.Player1)
        return Player1Markers.Contains (marker);
      return Player2Markers.Contains (marker);
    }


    // Check if game evaluation a is worse than b.
    private bool EvaluationWorseThan (GameEvaluation a, GameEvaluation b)
    {
      switch (a.status)
      {
      case Status.Win:
        if (b.status == Status.Win)
          return a.distance > b.distance;
        return false;
      case Status.Draw:
        if (b.status == Status.Win)
          return true;
        if (b.status == Status.Draw)
          return a.distance < b.distance;
        return false;
      case Status.Unresolved:
        if (b.status == Status.Win || b.status == Status.Draw)
          return true;
        if (b.status == Status.Unresolved)
          return a.distance < b.distance;
        return false;
      case Status.Loss:
        if (b.status != Status.Loss)
          return true;
        return a.distance < b.distance;
      default:
        return false;
      }
    }


    // Find the best move in the current position, returns false if game has ended.
    private bool FindBestMove (ref int row, ref int col, ref int marker)
    {
      if (CheckGameStatus (gameBoard) != Status.Unresolved)
        return false;
      GameAnalysis [0].CopyFrom (gameBoard);
      GameEvaluation evaluation = EvaluatePosition (0);
      row = evaluation.row;
      col = evaluation.col;
      marker = evaluation.marker;
      return true;
    }


    // Convert worst possible evaluation for opponent on next move
    // to evaluation for you on this move.
    private void FinalizeEvaluation (ref GameEvaluation a)
    {
      switch (a.status)
      {
      case Status.Win:
        a.status = Status.Loss;
        break;
      case Status.Loss:
        a.status = Status.Win;
        break;
      default:
        break;
      }
      a.distance++;
    }


    // Evaluate position 'currentDepth' in the 'GameAnalysis' array of game positions
    private GameEvaluation EvaluatePosition (int currentDepth)
    {
      int ID = GameAnalysis [currentDepth].GetID ();
      if (StateSpace [ID].status != Status.Unresolved)
        return StateSpace [ID]; // If position already evaluated, return saved value

      GameEvaluation result = new GameEvaluation (Status.Unresolved);
      result.status = CheckGameStatus (GameAnalysis [currentDepth]);
      if (result.status != Status.Unresolved)
      {
        result.distance = 0;
        StateSpace [ID] = result;
        return result; // return if game has ended.
      }
      GameEvaluation evaluation = new GameEvaluation (Status.Win);
      int [] markers = GameAnalysis [currentDepth].ActivePlayer == Player.Player1 ?
                       Player1Markers : Player2Markers;
      int newDepth = currentDepth + 1;

      // Find the worst position for your opponent after any of your moves,
      foreach (int marker in markers)
        for (int i = 0; i < 3; i++)
          for (int j = 0; j < 3; j++)
          {
            GameAnalysis [newDepth].CopyFrom (GameAnalysis [currentDepth]);
            if (GameAnalysis [newDepth].MakeMove (i, j, marker))
            {
              result = EvaluatePosition (newDepth);
              if (EvaluationWorseThan (result, evaluation))
              {
                evaluation = result;
                evaluation.row = i;
                evaluation.col = j;
                evaluation.marker = marker;
              }
            }
          }
      FinalizeEvaluation (ref evaluation);
      StateSpace [ID] = evaluation;
      return evaluation;
    }
  }

﻿//========================================================================================
// Class GameTypes
﻿// Different types of evaluation functions etc. for Tic Tac Toe variants.
//========================================================================================

  static class GameTypes
  {
    // Marker sets.
    public static readonly int [] MarkersX = new [] {1};
    public static readonly int [] MarkersO = new [] {2};
    public static readonly int [] MarkersXO = new [] {1, 2};


    // Board status evaluation for Regular variant.
    public static Status StatusRegular (TicTacToeBoard board)
    {
      if (board.Has3InARow ())
        return Status.Loss; // you lose if on your turn the board already has a 3 in a row
      if (board.IsFilled ())
        return Status.Draw;
      return Status.Unresolved;
    }


    // Board status evaluation for Misere variant (player to get 3 in a row loses)
    public static Status StatusMisere (TicTacToeBoard board)
    {
      if (board.Has3InARow ())
        return Status.Win; // opposite of regular game
      if (board.IsFilled ())
        return Status.Draw;
      return Status.Unresolved;
    }
  }

﻿//========================================================================================
// Class Program
//========================================================================================

  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main ()
    {
      Application.EnableVisualStyles ();
      Application.SetCompatibleTextRenderingDefault (false);
      Application.Run (new MainForm ());
    }
  }
}
