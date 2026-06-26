using UnityEngine;
using SownInStone.Core;

namespace SownInStone.Interactions
{
    /// <summary>
    /// Mini-game đá gà 3 hiệp đơn giản, dùng OnGUI (đồng bộ với hệ thống menu hiện tại).
    /// Kéo thả script này vào bất kỳ GameObject nào trong scene, hoặc CockfightingZone sẽ tự tạo.
    /// </summary>
    public class CockfightingMinigame : MonoBehaviour
    {
        // ─── Enum ─────────────────────────────────────────────────────────────
        private enum Move { TanCong, PhongThu, NeTranhg }
        private enum GameState { Idle, Playing, ShowResult }

        // ─── State ────────────────────────────────────────────────────────────
        private GameState state       = GameState.Idle;
        private int playerScore       = 0;
        private int chickenScore      = 0;
        private int currentRound      = 0;
        private const int TotalRounds = 3;

        private Move  playerMove;
        private Move  chickenMove;
        private string roundResultText = "";
        private string finalResultText = "";

        // ─── Style cache ──────────────────────────────────────────────────────
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle btnStyle;
        private GUIStyle resultStyle;
        private bool stylesBuilt = false;

        // ─────────────────────────────────────────────────────────────────────
        //  PUBLIC API
        // ─────────────────────────────────────────────────────────────────────
        public void StartMinigame()
        {
            playerScore      = 0;
            chickenScore     = 0;
            currentRound     = 1;
            roundResultText  = "";
            finalResultText  = "";
            state            = GameState.Playing;

            // Trả cursor để player click được nút
            CameraFollow3D.Instance?.ReleaseCursor();

            // Dừng thời gian game trong lúc đá gà
            Time.timeScale = 0f;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  ONGUI
        // ─────────────────────────────────────────────────────────────────────
        private void OnGUI()
        {
            if (state == GameState.Idle) return;

            BuildStyles();

            float panelW = 480f;
            float panelH = (state == GameState.ShowResult) ? 260f : 340f;
            float panelX = (Screen.width  - panelW) * 0.5f;
            float panelY = (Screen.height - panelH) * 0.5f;

            // Nền tối mờ toàn màn hình
            GUI.color = new Color(0f, 0f, 0f, 0.65f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Panel chính
            GUI.color = new Color(0.13f, 0.08f, 0.04f, 0.98f);
            GUI.DrawTexture(new Rect(panelX, panelY, panelW, panelH), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUILayout.BeginArea(new Rect(panelX + 20, panelY + 16, panelW - 40, panelH - 32));

            if (state == GameState.Playing) DrawPlayingUI();
            else DrawResultUI();

            GUILayout.EndArea();
        }

        private void DrawPlayingUI()
        {
            // Tiêu đề
            GUI.color = new Color(1f, 0.85f, 0.25f);
            GUILayout.Label("\U0001f413  ĐÁ GÀ!", titleStyle);
            GUI.color = Color.white;

            GUILayout.Space(4f);

            // Tỉ số
            string scoreStr = $"Hiệp {currentRound}/{TotalRounds}   |   " +
                              $"Thành {playerScore}  :  {chickenScore} Gà";
            GUILayout.Label(scoreStr, bodyStyle);

            GUILayout.Space(8f);

            // Kết quả hiệp trước
            if (!string.IsNullOrEmpty(roundResultText))
            {
                GUILayout.Label(roundResultText, bodyStyle);
                GUILayout.Space(6f);
            }

            GUILayout.Label("<b>Chọn nước đi:</b>", bodyStyle);
            GUILayout.Space(6f);

            // Nút chọn
            GUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color(0.85f, 0.25f, 0.1f);
            if (GUILayout.Button("\U0001f94a Tấn công", btnStyle, GUILayout.Height(44)))
                ResolveRound(Move.TanCong);

            GUI.backgroundColor = new Color(0.15f, 0.45f, 0.75f);
            if (GUILayout.Button("\U0001f6e1 Phòng thủ", btnStyle, GUILayout.Height(44)))
                ResolveRound(Move.PhongThu);

            GUI.backgroundColor = new Color(0.2f, 0.62f, 0.22f);
            if (GUILayout.Button("\U0001f4a8 Né tránh", btnStyle, GUILayout.Height(44)))
                ResolveRound(Move.NeTranhg);

            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();

            GUILayout.Space(12f);

            // Thoát
            GUI.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
            if (GUILayout.Button("Bỏ cuộc", btnStyle, GUILayout.Height(34)))
                EndMinigame(false, forced: true);
            GUI.backgroundColor = Color.white;
        }

        private void DrawResultUI()
        {
            bool playerWon = playerScore > chickenScore;

            GUI.color = playerWon ? new Color(0.3f, 1f, 0.4f) : new Color(1f, 0.35f, 0.25f);
            GUILayout.Label(playerWon ? "\U0001f3c6  THẮNG!" : "\U0001f480  THUA RỒI!", titleStyle);
            GUI.color = Color.white;

            GUILayout.Space(8f);
            GUILayout.Label(finalResultText, bodyStyle);
            GUILayout.Space(8f);

            if (playerWon)
            {
                GUI.color = new Color(1f, 0.88f, 0.28f);
                GUILayout.Label("+5 Tinh thần (Morale)", bodyStyle);
                GUI.color = Color.white;
            }

            GUILayout.Space(16f);

            GUI.backgroundColor = new Color(0.28f, 0.28f, 0.28f);
            if (GUILayout.Button("Đóng", btnStyle, GUILayout.Height(40)))
                EndMinigame(playerWon);
            GUI.backgroundColor = Color.white;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  GAME LOGIC
        // ─────────────────────────────────────────────────────────────────────
        /// <summary>Kéo búa bao - kéo búa: TanCong > NeTranhg > PhongThu > TanCong</summary>
        private void ResolveRound(Move player)
        {
            playerMove  = player;
            chickenMove = (Move)Random.Range(0, 3);

            int outcome = GetOutcome(playerMove, chickenMove);   // 1=win, -1=lose, 0=tie

            string moveNames(Move m) => m switch
            {
                Move.TanCong   => "Tấn công",
                Move.PhongThu  => "Phòng thủ",
                _              => "Né tránh"
            };

            string chickenEmoji = chickenMove switch
            {
                Move.TanCong  => "\U0001f94a",
                Move.PhongThu => "\U0001f6e1",
                _             => "\U0001f4a8"
            };

            if (outcome == 1)
            {
                playerScore++;
                roundResultText = $"Hiệp {currentRound}: Gà đối thủ {chickenEmoji} {moveNames(chickenMove)} → <color=#88ff88>Thành THẮNG!</color>";
            }
            else if (outcome == -1)
            {
                chickenScore++;
                roundResultText = $"Hiệp {currentRound}: Gà đối thủ {chickenEmoji} {moveNames(chickenMove)} → <color=#ff8888>Gà THẮNG!</color>";
            }
            else
            {
                roundResultText = $"Hiệp {currentRound}: Gà đối thủ {chickenEmoji} {moveNames(chickenMove)} → <color=#ffdd44>Hòa!</color>";
            }

            SownInStone.Audio.AudioManager.Instance?.PlaySFX("sfx_click");

            currentRound++;

            // Kiểm tra kết thúc sớm (có người đã thắng 2 hiệp)
            bool earlyEnd = playerScore >= 2 || chickenScore >= 2;
            if (currentRound > TotalRounds || earlyEnd)
            {
                bool playerWon = playerScore > chickenScore;
                finalResultText = $"Kết quả: Thành {playerScore} — {chickenScore} Gà\n{roundResultText}";
                state = GameState.ShowResult;

                if (playerWon)
                {
                    PlayerStats.Instance?.ModifyMorale(5f);
                }
            }
        }

        /// <returns>1=player wins, -1=chicken wins, 0=tie</returns>
        private int GetOutcome(Move p, Move c)
        {
            if (p == c) return 0;
            // TanCong > NeTranhg, NeTranhg > PhongThu, PhongThu > TanCong
            if ((p == Move.TanCong  && c == Move.NeTranhg) ||
                (p == Move.NeTranhg && c == Move.PhongThu)  ||
                (p == Move.PhongThu && c == Move.TanCong))
                return 1;
            return -1;
        }

        private void EndMinigame(bool playerWon, bool forced = false)
        {
            state          = GameState.Idle;
            Time.timeScale = 1f;

            // Khóa lại cursor theo camera mode
            CameraFollow3D.Instance?.SetCameraMode(
                CameraFollow3D.Instance.CurrentMode
            );
        }

        // ─────────────────────────────────────────────────────────────────────
        //  STYLE BUILDER
        // ─────────────────────────────────────────────────────────────────────
        private void BuildStyles()
        {
            if (stylesBuilt) return;
            stylesBuilt = true;

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 26,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                richText  = true
            };
            titleStyle.normal.textColor = new Color(1f, 0.85f, 0.25f);

            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 15,
                fontStyle = FontStyle.Normal,
                wordWrap  = true,
                richText  = true
            };
            bodyStyle.normal.textColor = new Color(0.92f, 0.88f, 0.80f);

            btnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize  = 15,
                fontStyle = FontStyle.Bold,
                richText  = true
            };
            btnStyle.normal.textColor = Color.white;
        }
    }
}
