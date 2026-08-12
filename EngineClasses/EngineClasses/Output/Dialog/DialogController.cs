// ============================================================
//  DialogController.cs  (Engine library)
//  Drives a single live conversation.
//
//  The controller is a state machine with three sub-states:
//    Reading  — showing the current line, waiting for advance
//    Choosing — all lines read, showing the choice list
//    Closed   — conversation over (IsOpen == false)
//
//  The DialogBox (game side) reads from the controller and
//  calls Advance() / SelectChoice() based on player input.
//  The controller never touches rendering or input directly.
// ============================================================

using System.Collections.Generic;

namespace ZeEngine
{
    public enum DialogPhase { Reading, Choosing, Closed }

    public class DialogController
    {
        // ── References ────────────────────────────────────────────────────────
        private DialogTree _tree;

        // ── Current state ─────────────────────────────────────────────────────
        public DialogPhase   Phase           { get; private set; } = DialogPhase.Closed;
        public DialogNode    CurrentNode     { get; private set; }
        public int           CurrentLineIndex { get; private set; }
        public List<DialogChoice> VisibleChoices { get; private set; } = new List<DialogChoice>();

        // Navigation history for "BACK" support
        private readonly Stack<string> _history = new Stack<string>();

        // ── Public API ────────────────────────────────────────────────────────

        public bool IsOpen => Phase != DialogPhase.Closed;

        /// <summary>Current line text (empty if no current node/line).</summary>
        public string CurrentLine
        {
            get
            {
                if (CurrentNode == null) return "";
                if (CurrentLineIndex >= CurrentNode.Lines.Count) return "";
                return CurrentNode.Lines[CurrentLineIndex];
            }
        }

        /// <summary>Current speaker name.</summary>
        public string Speaker => CurrentNode?.Speaker ?? "";

        /// <summary>
        /// Portrait path: node override → tree default → empty.
        /// The game-side DialogBox resolves this to a Texture2D.
        /// </summary>
        public string Portrait
        {
            get
            {
                if (!string.IsNullOrEmpty(CurrentNode?.Portrait)) return CurrentNode.Portrait;
                return _tree?.DefaultPortrait ?? "";
            }
        }

        // ── Open / Close ──────────────────────────────────────────────────────

        /// <summary>
        /// Start a conversation from a tree's "start" node.
        /// </summary>
        public void Open(DialogTree tree)
        {
            _tree   = tree;
            _history.Clear();
            GoToNode("start");
        }

        /// <summary>Close the dialog immediately.</summary>
        public void Close()
        {
            Phase       = DialogPhase.Closed;
            CurrentNode = null;
        }

        // ── Advance ───────────────────────────────────────────────────────────

        /// <summary>
        /// Called when the player presses Space/Enter/clicks the dialog box.
        /// In Reading phase: advance to next line, or move to Choosing/autoNext.
        /// In Choosing phase: do nothing (player must click a choice).
        /// </summary>
        public void Advance()
        {
            if (Phase == DialogPhase.Closed) return;

            if (Phase == DialogPhase.Reading)
            {
                int nextLine = CurrentLineIndex + 1;

                if (nextLine < CurrentNode.Lines.Count)
                {
                    // More lines in this node
                    CurrentLineIndex = nextLine;
                }
                else
                {
                    // All lines read
                    if (VisibleChoices.Count > 0)
                    {
                        Phase = DialogPhase.Choosing;
                    }
                    else if (!string.IsNullOrEmpty(CurrentNode.AutoNextNodeId))
                    {
                        GoToNode(CurrentNode.AutoNextNodeId);
                    }
                    else
                    {
                        Close();
                    }
                }
            }
            // In Choosing phase, Advance() is a no-op — use SelectChoice()
        }

        // ── Select choice ─────────────────────────────────────────────────────

        /// <summary>
        /// Called when the player clicks choice at index i in VisibleChoices.
        /// </summary>
        public void SelectChoice(int index)
        {
            if (Phase != DialogPhase.Choosing) return;
            if (index < 0 || index >= VisibleChoices.Count) return;

            DialogChoice choice = VisibleChoices[index];

            // Apply choice's setFlag side effect
            if (!string.IsNullOrEmpty(choice.SetFlag))
                _tree.SetFlag(choice.SetFlag);

            GoToNode(choice.NextNodeId);
        }

        // ── Navigation ────────────────────────────────────────────────────────

        private void GoToNode(string nodeId)
        {
            if (nodeId == "END" || string.IsNullOrEmpty(nodeId))
            {
                Close();
                return;
            }

            if (nodeId == "BACK")
            {
                if (_history.Count > 0)
                    nodeId = _history.Pop();
                else
                {
                    Close();
                    return;
                }
            }

            DialogNode node = _tree.GetNode(nodeId);
            if (node == null)
            {
                // Node not found or flag-gated — treat as END
                Close();
                return;
            }

            // Push current node to history (for BACK)
            if (CurrentNode != null)
                _history.Push(CurrentNode.Id);

            // Apply setFlagOnEnter
            if (!string.IsNullOrEmpty(node.SetFlagOnEnter))
                _tree.SetFlag(node.SetFlagOnEnter);

            CurrentNode      = node;
            CurrentLineIndex = 0;
            Phase            = DialogPhase.Reading;

            // Pre-compute visible choices (flag-filtered)
            VisibleChoices = _tree.FilterChoices(node.Choices);

            // If no lines at all, skip straight to choices / autoNext
            if (node.Lines.Count == 0)
                Advance();
        }
    }
}