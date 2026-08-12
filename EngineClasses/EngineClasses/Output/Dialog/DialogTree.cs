// ============================================================
//  DialogTree.cs  (Engine library)
//  A complete dialog tree for one NPC (or one conversation).
//  Owns the node dictionary and the flag state for this
//  particular conversation instance.
//
//  Flags are simple string tokens stored in a HashSet.
//  They persist for the lifetime of the tree instance.
//  If you want flags to persist across sessions, serialize
//  FlagState to your Save object.
// ============================================================

using System.Collections.Generic;

namespace ZeEngine
{
    public class DialogTree
    {
        // ── Identity ─────────────────────────────────────────────────────────
        /// <summary>ID matching the XML <Dialog id="..."> attribute.</summary>
        public string Id { get; }

        /// <summary>Default portrait path for all nodes that don't override it.</summary>
        public string DefaultPortrait { get; set; }

        // ── Nodes ─────────────────────────────────────────────────────────────
        private readonly Dictionary<string, DialogNode> _nodes
            = new Dictionary<string, DialogNode>();

        // ── Flag state ────────────────────────────────────────────────────────
        /// <summary>
        /// Flags set during this conversation.
        /// Serialise this to Save if you want persistent dialog state.
        /// </summary>
        public HashSet<string> FlagState { get; } = new HashSet<string>();

        // ── Constructor ───────────────────────────────────────────────────────

        public DialogTree(string id, string defaultPortrait = "")
        {
            Id             = id;
            DefaultPortrait = defaultPortrait;
        }

        // ── Node management ───────────────────────────────────────────────────

        public void AddNode(DialogNode node) => _nodes[node.Id] = node;

        /// <summary>
        /// Get a node by ID, respecting its RequireFlag condition.
        /// Returns null if the node doesn't exist or its flag requirement fails.
        /// </summary>
        public DialogNode GetNode(string id)
        {
            if (string.IsNullOrEmpty(id) || id == "END") return null;
            if (!_nodes.TryGetValue(id, out var node)) return null;

            // Check node-level flag requirement
            if (!string.IsNullOrEmpty(node.RequireFlag) && !CheckFlag(node.RequireFlag))
                return null;

            return node;
        }

        /// <summary>The entry point node — always named "start".</summary>
        public DialogNode StartNode => GetNode("start");

        // ── Flag helpers ──────────────────────────────────────────────────────

        public void SetFlag(string flag)
        {
            if (!string.IsNullOrEmpty(flag))
                FlagState.Add(flag);
        }

        public bool HasFlag(string flag) => FlagState.Contains(flag);

        /// <summary>
        /// Evaluates a flag expression:
        ///   "met_player"  → true if flag is set
        ///   "!met_player" → true if flag is NOT set
        ///   ""            → always true
        /// </summary>
        public bool CheckFlag(string flagExpr)
        {
            if (string.IsNullOrEmpty(flagExpr)) return true;
            if (flagExpr.StartsWith("!"))
                return !FlagState.Contains(flagExpr.Substring(1));
            return FlagState.Contains(flagExpr);
        }

        /// <summary>
        /// Filter a list of choices to only those whose RequireFlag is satisfied.
        /// </summary>
        public List<DialogChoice> FilterChoices(List<DialogChoice> choices)
        {
            var result = new List<DialogChoice>();
            foreach (var c in choices)
                if (CheckFlag(c.RequireFlag))
                    result.Add(c);
            return result;
        }
    }
}