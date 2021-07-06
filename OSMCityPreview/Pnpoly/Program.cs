using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pnpoly {
    class Program {
        struct Node {
            public float x, y;
            public Node(float x, float y) {
                this.x = x;
                this.y = y;
            }
        }
        const int undefined = 1073741824;
        static Node[] triangle = { new Node(44, 14), new Node(44, 30), new Node(20, 30) };
        static Node[] nodes = { new Node(20, 108), new Node(62, 108), new Node(60, 94), new Node(84, 94), new Node(84, 14), new Node(44, 14), new Node(44, 30), new Node(20, 30) };
        static Node[] tests = { new Node(44, 30), new Node(44, 29), new Node(44, 31), new Node(43, 30), new Node(45, 30) };
        static void Main(string[] args) {
            List<Node> triangle_list = new List<Node>();
            List<Node> node_list = new List<Node>();
            foreach (var node in triangle)
                triangle_list.Add(node);
            foreach (var node in nodes)
                node_list.Add(node);
            foreach (var test in tests) {
                Console.WriteLine("Pnpoly2 Polygon of (" + test.x + "," + test.y + ") is " + pnpoly2(node_list, test));
                Console.WriteLine("Pnpoly4 Polygon of (" + test.x + "," + test.y + ") is " + pnpoly4(node_list, test));
                Console.WriteLine("Pnpoly3 Triangle of (" + test.x + "," + test.y + ") is " + pnpoly3(triangle_list, test));
                Console.WriteLine();
            }
        }

        static bool pnpoly(List<Node> nodes, Node test) {
            bool c = false;
            int n = 0;
            foreach (var node in nodes) {
                n++;
            }
            for (int i = 0; i < n; i++) {
                int j = (i == 0) ? (n - 1) : (i - 1);
                if (((nodes[i].y > test.y) != (nodes[j].y > test.y)) && (test.x < (nodes[j].x - nodes[i].x) * (test.y - nodes[i].y) / (nodes[j].y - nodes[i].y) + nodes[i].x)) {
                    c = !c;
                }
            }
            return c;
        }
        static bool pnpoly2(List<Node> nodes, Node test) {
            bool c = false;
            int n = nodes.Count;
            for (int i = 0; i < n; i++) {
                //int j = (i == 0) ? (n - 1) : (i - 1);
                int j;
                if (i == 0 || nodes[i - 1].x == undefined || nodes[i - 1].y == undefined) {
                    for (j = i + 1; j < nodes.Count; j++) {
                        if (nodes[j].x == undefined || nodes[j].y == undefined) {
                            break;
                        }
                    }
                    j--;
                } else {
                    j = i - 1;
                }
                if (nodes[i].x == undefined || nodes[j].x == undefined) continue;
                if (((nodes[i].y > test.y) != (nodes[j].y > test.y)) && (test.x < (nodes[j].x - nodes[i].x) * (test.y - nodes[i].y) / (nodes[j].y - nodes[i].y) + nodes[i].x)) {
                    c = !c;
                }
            }
            return c;
        }
        static bool pnpoly3(List<Node> nodes, Node test) {
            bool c = false;
            int n = nodes.Count;
            for (int i = 0; i < n; i++) {
                int j = (i == 0) ? (n - 1) : (i - 1);
                if (nodes[i].x == test.x && test.x == nodes[j].x && Math.Min(nodes[i].y, nodes[j].y) <= test.y && test.y <= Math.Max(nodes[i].y, nodes[j].y)
                    || nodes[i].y == test.y && test.y == nodes[j].y && Math.Min(nodes[i].x, nodes[j].x) <= test.x && test.x <= Math.Max(nodes[i].x, nodes[j].x))
                    return true;
                if (((nodes[i].y > test.y) != (nodes[j].y > test.y)) && (test.x < (nodes[j].x - nodes[i].x) * (test.y - nodes[i].y) / (nodes[j].y - nodes[i].y) + nodes[i].x)) {
                    c = !c;
                }
            }
            return c;
        }
        static bool pnpoly4(List<Node> nodes, Node test) {
            bool c = false;
            int n = nodes.Count;
            for (int i = 0; i < n; i++) {
                //int j = (i == 0) ? (n - 1) : (i - 1);
                int j;
                if (i == 0 || nodes[i - 1].x == undefined || nodes[i - 1].y == undefined) {
                    for (j = i + 1; j < nodes.Count; j++) {
                        if (nodes[j].x == undefined || nodes[j].y == undefined) {
                            break;
                        }
                    }
                    j--;
                } else {
                    j = i - 1;
                }
                if (nodes[i].x == undefined || nodes[j].x == undefined) continue;
                if (nodes[i].x == test.x && test.x == nodes[j].x && Math.Min(nodes[i].y, nodes[j].y) <= test.y && test.y <= Math.Max(nodes[i].y, nodes[j].y)
                    || nodes[i].y == test.y && test.y == nodes[j].y && Math.Min(nodes[i].x, nodes[j].x) <= test.x && test.x <= Math.Max(nodes[i].x, nodes[j].x))
                    return false;
                if (((nodes[i].y > test.y) != (nodes[j].y > test.y)) && (test.x < (nodes[j].x - nodes[i].x) * (test.y - nodes[i].y) / (nodes[j].y - nodes[i].y) + nodes[i].x)) {
                    c = !c;
                }
            }
            return c;
        }
    }
}