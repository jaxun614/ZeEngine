// ============================================================
//  DialogLoader.cs  (Engine library)
//  Parses dialog XML files into DialogTree objects.
//
//  XML SCHEMA
//  ----------
//  <Dialogs>
//    <Dialog id="villager_intro" portrait="2D\NPCs\Portraits\OldMan">
//
//      <Node id="start" speaker="Old Man" setFlagOnEnter="met_old_man">
//        <Line>Traveller! You have arrived at last.</Line>
//        <Line>I have been waiting for someone like you.</Line>
//        <Choice text="Who are you?" next="who_are_you" />
//        <Choice text="What is this place?" next="this_place" />
//        <Choice text="Goodbye." next="END" />
//      </Node>
//
//      <Node id="who_are_you" speaker="Old Man">
//        <Line>I am the keeper of this island.</Line>
//        <Choice text="Tell me more." next="keeper_more" />
//        <Choice text="Back." next="start" />
//      </Node>
//
//      <!-- Flag-gated node: only accessible after quest is accepted -->
//      <Node id="quest_progress" speaker="Old Man" require="quest_accepted">
//        <Line>Have you found the stone yet?</Line>
//        <Choice text="Not yet." next="END" />
//        <Choice text="Yes, here it is." next="quest_complete"
//                require="has_stone" setFlag="quest_done" />
//      </Node>
//
//      <!-- Auto-advance: no choices, jumps straight to next node -->
//      <Node id="keeper_more" speaker="Old Man" autoNext="start">
//        <Line>This island holds ancient secrets...</Line>
//      </Node>
//
//    </Dialog>
//  </Dialogs>
//
//  ATTRIBUTE REFERENCE
//  -------------------
//  Dialog:   id, portrait (optional default portrait path)
//  Node:     id, speaker, portrait (overrides dialog default),
//            require (flag expression), setFlagOnEnter, autoNext
//  Choice:   text, next (node id or END), require, setFlag
//  Line:     inner text content
// ============================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace ZeEngine
{
    public static class DialogLoader
    {
        // ── Load all dialogs from one file ────────────────────────────────────

        /// <summary>
        /// Load every &lt;Dialog&gt; from an XML file.
        /// Returns a dictionary keyed by dialog ID.
        /// Throws descriptive exceptions on malformed XML.
        /// </summary>
        public static Dictionary<string, DialogTree> LoadFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"DialogLoader: file not found: {filePath}");

            XDocument doc;
            try { doc = XDocument.Load(filePath); }
            catch (Exception ex)
            {
                throw new InvalidDataException(
                    $"DialogLoader: failed to parse XML at '{filePath}': {ex.Message}", ex);
            }

            var result = new Dictionary<string, DialogTree>();

            XElement root = doc.Root;
            if (root == null)
                throw new InvalidDataException("DialogLoader: XML has no root element.");

            // Root may be <Dialogs> wrapping multiple <Dialog> elements,
            // or a single <Dialog> as the root.
            IEnumerable<XElement> dialogElements =
                root.Name.LocalName == "Dialog"
                    ? new[] { root }
                    : root.Elements("Dialog");

            foreach (XElement dialogEl in dialogElements)
            {
                DialogTree tree = ParseDialog(dialogEl, filePath);
                result[tree.Id] = tree;
            }

            return result;
        }

        /// <summary>
        /// Load a single dialog by ID from a file.
        /// Returns null if no dialog with that ID exists in the file.
        /// </summary>
        public static DialogTree LoadOne(string filePath, string dialogId)
        {
            var all = LoadFile(filePath);
            return all.TryGetValue(dialogId, out var t) ? t : null;
        }

        // ── Parse helpers ─────────────────────────────────────────────────────

        private static DialogTree ParseDialog(XElement el, string filePath)
        {
            string id = Attr(el, "id", filePath, required: true);
            string portrait = Attr(el, "portrait");

            var tree = new DialogTree(id, portrait);

            foreach (XElement nodeEl in el.Elements("Node"))
            {
                DialogNode node = ParseNode(nodeEl, filePath);
                tree.AddNode(node);
            }

            return tree;
        }

        private static DialogNode ParseNode(XElement el, string filePath)
        {
            string id      = Attr(el, "id",      filePath, required: true);
            string speaker = Attr(el, "speaker",  filePath, required: true);

            var node = new DialogNode(id, speaker)
            {
                Portrait        = Attr(el, "portrait"),
                RequireFlag     = Attr(el, "require"),
                SetFlagOnEnter  = Attr(el, "setFlagOnEnter"),
                AutoNextNodeId  = Attr(el, "autoNext"),
            };

            // Lines
            foreach (XElement lineEl in el.Elements("Line"))
            {
                string text = lineEl.Value.Trim();
                if (!string.IsNullOrEmpty(text))
                    node.Lines.Add(text);
            }

            // Choices
            foreach (XElement choiceEl in el.Elements("Choice"))
            {
                string choiceText = Attr(choiceEl, "text",    filePath, required: true);
                string next       = Attr(choiceEl, "next",    filePath, required: true);
                string require    = Attr(choiceEl, "require");
                string setFlag    = Attr(choiceEl, "setFlag");

                node.Choices.Add(new DialogChoice(choiceText, next, require, setFlag));
            }

            // Validation: a node with no choices and no autoNext is a dead end
            if (node.Choices.Count == 0 && string.IsNullOrEmpty(node.AutoNextNodeId))
            {
                // Treat as auto-END (graceful fallback)
                node.AutoNextNodeId = "END";
            }

            return node;
        }

        private static string Attr(XElement el, string name,
                                   string filePath = "", bool required = false)
        {
            var attr = el.Attribute(name);
            if (attr == null)
            {
                if (required)
                    throw new InvalidDataException(
                        $"DialogLoader: <{el.Name}> missing required attribute '{name}'" +
                        (string.IsNullOrEmpty(filePath) ? "" : $" in {filePath}"));
                return "";
            }
            return attr.Value.Trim();
        }
    }
}