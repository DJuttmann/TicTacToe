using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace TicTacToe
{
  public partial class MainForm: Form
  {
    enum PlayerType {human, computer}

    private TicTacToeGame Game;
    private Image ImageX;
    private Image ImageO;
    private Image ImageXFaded;
    private Image ImageOFaded;
    private string [] GameTypeNames = new []
    {
      "Regular",
      "Misere",
      "X only Misere",
      "Wild",
      "Wild Misere"
    };
    private string [] gameTypeDescriptions = new []
    {
      "Player 1 uses X, Player 2 uses O.\nFirst to make 3 in a row wins.",
      "Player 1 uses X, Player 2 uses O.\nFirst to make 3 in a row loses.",
      "Both players use X.\nFirst to make 3 in a row loses.",
      "Both players use X and O.\nFirst to make 3 in a row wins.",
      "Both players use X and O.\nFirst to make 3 in a row loses."
    };
    private string [] PlayerTypeNames = new []
    {
      "Human vs Computer",
      "Computer vs Human",
      "Human vs Human"
    };
    PlayerType Player1;
    PlayerType Player2;
    int GameType;
    int PlayerTypes;
    private int SelectedMarker;
    private bool AwaitingComputerMove = false;

    
    // Constructor.
    public MainForm ()
    {
      InitializeComponent ();
      Game = new TicTacToeGame ();
      try
      {
        ImageX = Image.FromFile ("MarkerX.png");
        ImageO = Image.FromFile ("MarkerO.png");
        ImageXFaded = Image.FromFile ("MarkerXFaded.png");
        ImageOFaded = Image.FromFile ("MarkerOFaded.png");
      }
      catch (Exception ex)
      {
        throw ex;
      }
      foreach (string str in GameTypeNames)
        listBoxGameType.Items.Add (str);
      listBoxGameType.SelectedIndex = 0;
      GameType = 0;
      foreach (string str in PlayerTypeNames)
        listBoxPlayerTypes.Items.Add (str);
      listBoxPlayerTypes.SelectedIndex = 0;
      PlayerTypes = 0;
      Player1 = PlayerType.human;
      Player2 = PlayerType.computer;
      SelectedMarker = 1;
      NewGame ();
    }


    // Updates the game type info box in on the game select menu.
    private void UpdateInfo (int index)
    {
      if (index >= 0 && index < gameTypeDescriptions.Length)
      labelDescription.Text = gameTypeDescriptions [index];
    }


    // Start a new game.
    private void NewGame ()
    {
      AwaitingComputerMove = false; // cancel any expected computer moves
      switch (PlayerTypes)
      {
      default:
        MessageBox.Show ("Something went wrong 1");
        break;
      case 0:
        Player1 = PlayerType.human;
        Player2 = PlayerType.computer;
        break;
      case 1:
        Player1 = PlayerType.computer;
        Player2 = PlayerType.human;
        break;
      case 2:
        Player1 = PlayerType.human;
        Player2 = PlayerType.human;
        break;
      }
      switch (GameType)
      {
      default:
        MessageBox.Show ("Something went wrong 2");
        break;
      case 0:
        Game.NewGame (GameTypes.StatusRegular, GameTypes.MarkersX, GameTypes.MarkersO);
        break;
      case 1:
        Game.NewGame (GameTypes.StatusMisere, GameTypes.MarkersX, GameTypes.MarkersO);
        break;
      case 2:
        Game.NewGame (GameTypes.StatusMisere, GameTypes.MarkersX, GameTypes.MarkersX);
        break;
      case 3:
        Game.NewGame (GameTypes.StatusRegular, GameTypes.MarkersXO, GameTypes.MarkersXO);
        break;
      case 4:
        Game.NewGame (GameTypes.StatusMisere, GameTypes.MarkersXO, GameTypes.MarkersXO);
        break;
      }
      if (Player1 == PlayerType.computer)
      {
        AwaitingComputerMove = true; // set flag that you are expecting a computer move
        MakeComputerMove ();
      }
      else
      {
        RenderBoard ();
        UpdateGameStatus ();
      }
    }


    // Display game mode, game status, active player.
    public void UpdateGameStatus ()
    {
      string s = "Mode: " + GameTypeNames [GameType] + "\n";
      switch (Game.Status)
      {
      case Status.Unresolved:
        SetPlayerMarkers ();
        if (Game.ActivePlayer == Player.Player1)
          labelStatus.Text = s + "Player 1 to move";
        else
          labelStatus.Text = s + "Player 2 to move";
        break;
      case Status.Draw:
        labelStatus.Text = s + "Draw";
        break;
      case Status.Win:
        if (Game.ActivePlayer == Player.Player1)
          labelStatus.Text = s + "Player 1 Wins";
        else
          labelStatus.Text = s + "Player 2 Wins";
        break;
      case Status.Loss:
        if (Game.ActivePlayer == Player.Player1)
          labelStatus.Text = s + "Player 2 Wins";
        else
          labelStatus.Text = s + "Player 1 Wins";
        break;
      default:
        break;
      }
    }


    // Select and display marker for current player in wild games
    private void SelectMarker (int i)
    {
      switch (i)
      {
      case 1:
        SelectedMarker = 1;
        selectMarkerX.Image = ImageX;
        selectMarkerO.Image = ImageOFaded;
        break;
      case 2:
        SelectedMarker = 2;
        selectMarkerX.Image = ImageXFaded;
        selectMarkerO.Image = ImageO;
        break;
      default:
        selectMarkerX.Image = null;
        selectMarkerO.Image = null;
        break;
      }
    }
  

    // Set which marker current player can use and has selected.
    private void SetPlayerMarkers ()
    {
      switch (GameType)
      {
      case 0: // Regular
        SelectMarker (0);
        SelectedMarker = Game.ActivePlayer == Player.Player1 ? 1 : 2;
        break;
      case 1: // Misere
        SelectMarker (0);
        SelectedMarker = Game.ActivePlayer == Player.Player1 ? 1 : 2;
        break;
      case 2: // X only misere
        SelectMarker (0);
        SelectedMarker = 1;
        break;
      case 3: // Wild
      case 4: // Wild misere
        SelectMarker (SelectedMarker);
        break;
      }
    }


    // Update the marker images on the board.
    private void RenderBoard ()
    {
      Image [,] fields = new Image [3, 3];
      for (int i = 0; i < 3; i++)
        for (int j = 0; j < 3; j++)
          switch (Game.GetField (i, j))
          {
          case 0: 
            fields [i, j] = null;
            break;
          case 1:
            fields [i, j] = ImageX;
            break;
          case 2:
            fields [i, j] = ImageO;
            break;
          }
      pictureBox_0_0.Image = fields [0, 0];
      pictureBox_0_1.Image = fields [0, 1];
      pictureBox_0_2.Image = fields [0, 2];
      pictureBox_1_0.Image = fields [1, 0];
      pictureBox_1_1.Image = fields [1, 1];
      pictureBox_1_2.Image = fields [1, 2];
      pictureBox_2_0.Image = fields [2, 0];
      pictureBox_2_1.Image = fields [2, 1];
      pictureBox_2_2.Image = fields [2, 2];
    }


    // Try to make a move by placing SelectedMarker at (row, col).
    private async void MakeMove (int row, int col)
    {
      if (AwaitingComputerMove)
        return;
      if (Game.MakeMove (row, col, SelectedMarker))
      {
        if ((Game.ActivePlayer == Player.Player1 && Player1 == PlayerType.computer) ||
            (Game.ActivePlayer == Player.Player2 && Player2 == PlayerType.computer))
        {
          RenderBoard ();
          UpdateGameStatus ();
          AwaitingComputerMove = true; // prevent making moves while waiting
          await Task.Delay (500);
          MakeComputerMove ();
        }
        RenderBoard ();
        UpdateGameStatus ();
      }
    }


    // Let the computer make a move for the current player.
    private void MakeComputerMove ()
    {
      if (!AwaitingComputerMove)
        return; // Cancel computer move if not expecting one.
      UpdateGameStatus ();
      if (Game.Status != Status.Unresolved)
      {
        return;
      }
      int row = 0, col = 0, marker = 0;
      if (!Game.MakeComputerMove (ref row, ref col, ref marker))
        return;
      RenderBoard ();
      UpdateGameStatus ();
      AwaitingComputerMove = false;
    }
    

    // Board click handlers.
    private void pictureBox_0_0_Click (object sender, EventArgs e)
    {
      MakeMove (0, 0);
    }

    private void pictureBox_0_1_Click (object sender, EventArgs e)
    {
      MakeMove (0, 1);
    }

    private void pictureBox_0_2_Click (object sender, EventArgs e)
    {
      MakeMove (0, 2);
    }

    private void pictureBox_1_0_Click (object sender, EventArgs e)
    {
      MakeMove (1, 0);
    }

    private void pictureBox_1_1_Click (object sender, EventArgs e)
    {
      MakeMove (1, 1);
    }

    private void pictureBox_1_2_Click (object sender, EventArgs e)
    {
      MakeMove (1, 2);
    }

    private void pictureBox_2_0_Click (object sender, EventArgs e)
    {
      MakeMove (2, 0);
    }

    private void pictureBox_2_1_Click (object sender, EventArgs e)
    {
      MakeMove (2, 1);
    }

    private void pictureBox_2_2_Click (object sender, EventArgs e)
    {
      MakeMove (2, 2);
    }


    private void buttonNewGame_Click (object sender, EventArgs e)
    {
      NewGame ();
    }


    private void listBoxGameType_SelectedIndexChanged (object sender, EventArgs e)
    {
      GameType = listBoxGameType.FindStringExact(listBoxGameType.Text);
    }


    private void selectPlayerTypes_SelectedIndexChanged (object sender, EventArgs e)
    {
      PlayerTypes = listBoxPlayerTypes.FindStringExact(listBoxPlayerTypes.Text);
    }


    private void buttonSelectGame_Click (object sender, EventArgs e)
    {
      buttonSelectGame.Enabled = false;
      buttonNewGame.Enabled = false;
      panelNewGame.Visible = true;
      listBoxGameType.SelectedIndex = GameType;
      listBoxPlayerTypes.SelectedIndex = PlayerTypes;
      UpdateInfo (GameType);
    }


    private void buttonGameSelect_Click (object sender, EventArgs e)
    {
      panelNewGame.Visible = false;
      buttonSelectGame.Enabled = true;
      buttonNewGame.Enabled = true;
      GameType = listBoxGameType.SelectedIndex;
      PlayerTypes = listBoxPlayerTypes.SelectedIndex;
      NewGame ();
    }


    private void buttonGameCancel_Click (object sender, EventArgs e)
    {
      panelNewGame.Visible = false;
      buttonSelectGame.Enabled = true;
      buttonNewGame.Enabled = true;
    }


    private void listBoxGameType_SelectedIndexChanged_1 (object sender, EventArgs e)
    {
      UpdateInfo (listBoxGameType.SelectedIndex);
    }


    private void selectMarkerX_Click (object sender, EventArgs e)
    {
      if (GameType == 3 || GameType == 4)
        SelectMarker (1);
    }

    private void selectMarkerO_Click (object sender, EventArgs e)
    {
      if (GameType == 3 || GameType == 4)
        SelectMarker (2);
    }
  }
}
