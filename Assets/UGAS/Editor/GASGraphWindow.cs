using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UnityGAS
{
    public class GASGraphWindow : EditorWindow
    {
        private AbilityDefinition selectedAbility;
        private readonly List<GraphNode> nodes = new List<GraphNode>();
        private readonly List<GraphConnection> connections = new List<GraphConnection>();

        private GUIStyle nodeStyle;
        private GUIStyle abilityNodeStyle;
        private GUIStyle effectNodeStyle;
        private GUIStyle attributeNodeStyle;

        [MenuItem("Window/GAS/Ability Visualizer")]
        public static void ShowWindow()
        {
            GetWindow<GASGraphWindow>("Ability Visualizer");
        }

        private void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;
            InitStyles();
            OnSelectionChanged();
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }

        private void InitStyles()
        {
            nodeStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 10, 10),
                border = new RectOffset(12, 12, 12, 12)
            };

            abilityNodeStyle = new GUIStyle(nodeStyle) { normal = { background = MakeTex(2, 2, new Color(0.1f, 0.2f, 0.4f)) } };
            effectNodeStyle = new GUIStyle(nodeStyle) { normal = { background = MakeTex(2, 2, new Color(0.4f, 0.1f, 0.2f)) } };
            attributeNodeStyle = new GUIStyle(nodeStyle) { normal = { background = MakeTex(2, 2, new Color(0.2f, 0.4f, 0.1f)) } };
        }

        private void OnGUI()
        {
            if (nodeStyle == null) InitStyles();

            EditorGUILayout.LabelField("Ability Visualizer", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Select an Ability Definition asset to visualize its effect relationships. This is a read-only view.", MessageType.Info);

            selectedAbility = (AbilityDefinition)EditorGUILayout.ObjectField("Selected Ability", selectedAbility, typeof(AbilityDefinition), false);

            if (GUI.changed)
            {
                BuildGraph();
            }

            DrawConnections();
            DrawNodes();

            Repaint();
        }

        private void OnSelectionChanged()
        {
            if (Selection.activeObject is AbilityDefinition ability)
            {
                selectedAbility = ability;
                BuildGraph();
            }
        }

        private void BuildGraph()
        {
            nodes.Clear();
            connections.Clear();

            if (selectedAbility == null) return;

            // Ability Node
            var abilityNode = new GraphNode(selectedAbility, new Rect(50, 200, 200, 50), NodeType.Ability);
            nodes.Add(abilityNode);

            // Effect Nodes
            for (int i = 0; i < selectedAbility.effects.Count; i++)
            {
                var effect = selectedAbility.effects[i];
                if (effect == null) continue;

                var effectNode = new GraphNode(effect, new Rect(350, 100 + i * 100, 200, 50), NodeType.Effect);
                nodes.Add(effectNode);
                connections.Add(new GraphConnection(abilityNode, effectNode));

                // Attribute Nodes
                foreach (var mod in effect.modifiers)
                {
                    if (mod.attribute == null) continue;
                    var existingAttrNode = nodes.FirstOrDefault(n => n.Asset == mod.attribute);
                    if (existingAttrNode != null)
                    {
                        connections.Add(new GraphConnection(effectNode, existingAttrNode));
                    }
                    else
                    {
                        var attrNode = new GraphNode(mod.attribute, new Rect(650, 100 + i * 100, 200, 50), NodeType.Attribute);
                        nodes.Add(attrNode);
                        connections.Add(new GraphConnection(effectNode, attrNode));
                    }
                }
            }
        }

        private void DrawNodes()
        {
            foreach (var node in nodes)
            {
                GUIStyle style = node.Type switch
                {
                    NodeType.Ability => abilityNodeStyle,
                    NodeType.Effect => effectNodeStyle,
                    NodeType.Attribute => attributeNodeStyle,
                    _ => nodeStyle
                };
                GUI.Box(node.Position, node.Asset.name, style);
            }
        }

        private void DrawConnections()
        {
            foreach (var conn in connections)
            {
                Handles.DrawBezier(
                    conn.From.Position.center + new Vector2(conn.From.Position.width / 2, 0),
                    conn.To.Position.center - new Vector2(conn.To.Position.width / 2, 0),
                    conn.From.Position.center + new Vector2(conn.From.Position.width / 2 + 50, 0),
                    conn.To.Position.center - new Vector2(conn.To.Position.width / 2 + 50, 0),
                    Color.white,
                    null,
                    2f
                );
            }
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private class GraphNode
        {
            public ScriptableObject Asset { get; }
            public Rect Position { get; }
            public NodeType Type { get; }

            public GraphNode(ScriptableObject asset, Rect position, NodeType type)
            {
                Asset = asset;
                Position = position;
                Type = type;
            }
        }

        private class GraphConnection
        {
            public GraphNode From { get; }
            public GraphNode To { get; }

            public GraphConnection(GraphNode from, GraphNode to)
            {
                From = from;
                To = to;
            }
        }

        private enum NodeType { Ability, Effect, Attribute }
    }
}