// ============================================================
//  DialogNode.cs  (Engine library)
//  A single node in a branching dialog tree.
//
//  Each node has:
//    - A speaker name (shown as the portrait label)
//    - One or more lines of text (shown sequentially)
//    - Zero or more choices (shown after all lines are read)
//    - Optional flag conditions / side effects
//
//  Special nextNode values:
//    "END"   — closes the dialog box
//    "BACK"  — returns to the previous node in the history stack
// ============================================================

using System.Collections.Generic;

namespace ZeEngine
{
    // ── Choice ────────────────────────────────────────────────────────────────

    public class DialogChoice
    {
        /// <summary>Text shown on the choice button.</summary>
        public string Text;

        /// <summary>ID of the node to jump to when this choice is selected.</summary>
        public string NextNodeId;

        /// <summary>
        /// Optional: this choice is only shown when the named flag is set.
        /// Empty string = always shown.
        /// Prefix with '!' to require the flag to be UNSET (e.g. "!met_player").
        /// </summary>
        public string RequireFlag;

        /// <summary>
        /// Optional: set this flag when this choice is selected.
        /// Empty string = no side effect.
        /// </summary>
        public string SetFlag;

        public DialogChoice(string text, string nextNodeId,
                            string requireFlag = "", string setFlag = "")
        {
            Text        = text;
            NextNodeId  = nextNodeId;
            RequireFlag = requireFlag;
            SetFlag     = setFlag;
        }
    }

    // ── Node ──────────────────────────────────────────────────────────────────

    public class DialogNode
    {
        /// <summary>Unique ID within the tree (e.g. "start", "quest_offered").</summary>
        public string Id;

        /// <summary>Name displayed above the dialog text (e.g. "Old Man").</summary>
        public string Speaker;

        /// <summary>
        /// Lines of text shown one at a time (advance with Space/Enter/click).
        /// Multiple lines let one node feel like a conversation beat.
        /// </summary>
        public List<string> Lines = new List<string>();

        /// <summary>Choices shown after all lines have been read.</summary>
        public List<DialogChoice> Choices = new List<DialogChoice>();

        /// <summary>
        /// Optional portrait content path (e.g. "2D\\NPCs\\Portraits\\OldMan").
        /// Falls back to the NPC's default portrait if empty.
        /// </summary>
        public string Portrait;

        /// <summary>
        /// Optional: this node is only reachable when this flag is set.
        /// Used by DialogTree.GetNode() to skip locked nodes.
        /// </summary>
        public string RequireFlag;

        /// <summary>
        /// Optional: set this flag when this node is first entered.
        /// Use for "met the NPC", "heard the rumour", etc.
        /// </summary>
        public string SetFlagOnEnter;

        /// <summary>
        /// When there are no choices, advance to this node automatically
        /// after all lines are read.  "END" closes; empty = show no choices
        /// but don't auto-advance (dead end — shouldn't happen in practice).
        /// </summary>
        public string AutoNextNodeId;

        public DialogNode(string id, string speaker)
        {
            Id      = id;
            Speaker = speaker;
        }
    }
}