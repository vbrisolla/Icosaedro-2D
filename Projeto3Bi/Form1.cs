using System;
using System.Drawing;
using System.Windows.Forms;

namespace IcosaedroCG
{
    public partial class Form1 : Form
    {
        // ==========================================================
        // CENTRO DO ICOSAEDRO DENTRO DO PAINEL DE DESENHO
        // (usado para a escala acontecer em relacao ao centro)
        // ==========================================================
        const int CX = 300;
        const int CY = 260;

        // ==========================================================
        // ESTRUTURAS DE DADOS DO POLIEDRO
        // ==========================================================
        Point[] vertices;      // 12 vertices (coordenadas 2D da projecao)
        int[,] arestas;        // 30 arestas (par de vertices)
        int[,] faces;          // 20 faces triangulares (trio de vertices)
        int[] facesVisiveis;   // 10 faces que aparecem na imagem

        Color[] coresFaces;    // cor atual de cada face
        Color corSelecionada;  // cor escolhida no mosaico

        // ==========================================================
        // TRANSFORMACOES
        // ==========================================================
        float translacaoX = 0;
        float translacaoY = 0;
        float escala = 1.0f;

        Font fonteNumero = new Font("Arial", 10, FontStyle.Bold);

        public Form1()
        {
            InitializeComponent();
            InicializarIcosaedro();
            InicializarCores();
        }

        // ==========================================================
        // PRIMITIVAS APRENDIDAS EM AULA
        // ==========================================================

        public Color cor(int r, int g, int b)
        {
            Color cor = new Color();
            cor = Color.FromArgb(r, g, b);

            return cor;
        }

        public Pen caneta(int r, int g, int b, float espessura)
        {
            Pen can = new Pen(cor(r, g, b), espessura);

            return can;
        }

        public void pintaLinha(PaintEventArgs e, Pen caneta, int x1, int y1, int x2, int y2)
        {
            e.Graphics.DrawLine(caneta, x1, y1, x2, y2);
        }

        public Point[] Poligono(int[] x, int[] y)
        {
            Point[] pontos = new Point[x.Length];

            for (int i = 0; i < x.Length; i++)
            {
                Point point1 = new Point(x[i], y[i]);
                pontos[i] = point1;
            }

            return pontos;
        }

        public void PreenchePoligono(PaintEventArgs e, SolidBrush fundo, Point[] pontos)
        {
            e.Graphics.FillPolygon(fundo, pontos);
        }

        public SolidBrush preen_Area(Color cor)
        {
            return new SolidBrush(cor);
        }

        // ==========================================================
        // 1) DEFINICAO DOS 12 VERTICES
        //    Raio do hexagono R = 180, centro (300, 260)
        //    Vertices internos ficam no raio r = R / 1.618 = 111
        // ==========================================================
        private void InicializarIcosaedro()
        {
            vertices = new Point[]
            {
                new Point(300,  80),   // 0  - topo do hexagono
                new Point(456, 170),   // 1  - superior direito
                new Point(456, 350),   // 2  - inferior direito
                new Point(300, 440),   // 3  - base do hexagono
                new Point(144, 350),   // 4  - inferior esquerdo
                new Point(144, 170),   // 5  - superior esquerdo

                new Point(396, 204),   // 6  - interno: ponta direita da horizontal
                new Point(204, 204),   // 7  - interno: ponta esquerda da horizontal
                new Point(300, 371),   // 8  - interno: vertice de baixo do triangulo

                new Point(300, 149),   // 9  - interno OCULTO (face de tras)
                new Point(204, 316),   // 10 - interno OCULTO (face de tras)
                new Point(396, 316)    // 11 - interno OCULTO (face de tras)
            };

            // ------------------------------------------------------
            // 2) AS 30 ARESTAS
            // ------------------------------------------------------
            arestas = new int[,]
            {
                // hexagono externo (6)
                {0,1},{1,2},{2,3},{3,4},{4,5},{5,0},

                // triangulo interno da frente (3)
                {6,7},{7,8},{8,6},

                // ligacoes radiais interno -> vertice alinhado (3)
                {6,1},{7,5},{8,3},

                // interno -> vertices vizinhos do hexagono (6)
                {6,0},{6,2},{7,0},{7,4},{8,2},{8,4},

                // arestas escondidas atras do solido (12)
                {9,0},{9,1},{9,5},{9,10},{9,11},
                {10,3},{10,4},{10,5},{10,11},
                {11,1},{11,2},{11,3}
            };

            // ------------------------------------------------------
            // 3) AS 20 FACES TRIANGULARES
            // ------------------------------------------------------
            faces = new int[,]
            {
                // ----- 10 faces da FRENTE (visiveis) -----
                {6,7,8},   // face 1  - triangulo central
                {0,7,6},   // face 2  - triangulo do topo
                {6,0,1},   // face 3
                {6,1,2},   // face 4
                {6,2,8},   // face 5
                {8,2,3},   // face 6
                {8,3,4},   // face 7
                {8,4,7},   // face 8
                {7,4,5},   // face 9
                {7,5,0},   // face 10

                // ----- 10 faces de TRAS (ocultas) -----
                {10,11,9},
                {3,11,10},
                {10,3,4},
                {10,4,5},
                {10,5,9},
                {9,5,0},
                {9,0,1},
                {9,1,11},
                {11,1,2},
                {11,2,3}
            };

            // ------------------------------------------------------
            // 4) AS 10 FACES VISIVEIS (indices dentro de "faces")
            // ------------------------------------------------------
            facesVisiveis = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        }

        private void InicializarCores()
        {
            coresFaces = new Color[20];

            for (int i = 0; i < 20; i++)
            {
                coresFaces[i] = cor(255, 255, 255);   // branco = sem pintura
            }

            corSelecionada = cor(255, 0, 0);
        }

        // ==========================================================
        // TRANSFORMACAO: ESCALA PELO CENTRO + TRANSLACAO
        // ==========================================================
        private Point TransformarPonto(Point ponto)
        {
            // 1) escala em relacao ao CENTRO do icosaedro
            float x = CX + (ponto.X - CX) * escala;
            float y = CY + (ponto.Y - CY) * escala;

            // 2) translacao
            x = x + translacaoX;
            y = y + translacaoY;

            return new Point((int)x, (int)y);
        }

        // Devolve os 3 pontos JA TRANSFORMADOS de uma face
        private Point[] PontosDaFace(int f)
        {
            int[] x = new int[3];
            int[] y = new int[3];

            for (int k = 0; k < 3; k++)
            {
                Point p = TransformarPonto(vertices[faces[f, k]]);
                x[k] = p.X;
                y[k] = p.Y;
            }

            return Poligono(x, y);
        }

        // ==========================================================
        // DESENHO
        // ==========================================================
        private void pnlDesenho_Paint(object sender, PaintEventArgs e)
        {
            DesenharIcosaedro(e);
        }

        private void DesenharIcosaedro(PaintEventArgs e)
        {
            DesenharFaces(e);
            DesenharArestas(e);
            DesenharNumeros(e);
        }

        private void DesenharFaces(PaintEventArgs e)
        {
            for (int i = 0; i < facesVisiveis.Length; i++)
            {
                int f = facesVisiveis[i];

                Point[] pontos = PontosDaFace(f);
                SolidBrush pincel = preen_Area(coresFaces[f]);

                PreenchePoligono(e, pincel, pontos);
            }
        }

        // Uma aresta so e desenhada se ela pertencer a alguma face visivel.
        // As outras estao atras do solido e nao aparecem na imagem.
        private bool ArestaEhVisivel(int a, int b)
        {
            for (int i = 0; i < facesVisiveis.Length; i++)
            {
                int f = facesVisiveis[i];

                int v1 = faces[f, 0];
                int v2 = faces[f, 1];
                int v3 = faces[f, 2];

                bool temA = (a == v1 || a == v2 || a == v3);
                bool temB = (b == v1 || b == v2 || b == v3);

                if (temA && temB)
                {
                    return true;
                }
            }

            return false;
        }

        private void DesenharArestas(PaintEventArgs e)
        {
            Pen can = caneta(0, 0, 0, 2);

            for (int i = 0; i < 30; i++)
            {
                int a = arestas[i, 0];
                int b = arestas[i, 1];

                if (ArestaEhVisivel(a, b))
                {
                    Point p1 = TransformarPonto(vertices[a]);
                    Point p2 = TransformarPonto(vertices[b]);

                    pintaLinha(e, can, p1.X, p1.Y, p2.X, p2.Y);
                }
            }
        }

        // Escreve o numero (1 a 10) no centro de cada face visivel
        private void DesenharNumeros(PaintEventArgs e)
        {
            SolidBrush pincel = preen_Area(cor(0, 0, 0));

            for (int i = 0; i < facesVisiveis.Length; i++)
            {
                Point[] p = PontosDaFace(facesVisiveis[i]);

                int cx = (p[0].X + p[1].X + p[2].X) / 3;
                int cy = (p[0].Y + p[1].Y + p[2].Y) / 3;

                e.Graphics.DrawString((i + 1).ToString(), fonteNumero, pincel, cx - 6, cy - 8);
            }
        }

        // ==========================================================
        // CLIQUE: DESCOBRIR QUAL FACE FOI CLICADA
        // ==========================================================

        // Calcula de que lado da reta AB o ponto P esta.
        // Positivo = um lado, negativo = o outro lado, zero = em cima da reta.
        private float Sinal(Point p, Point a, Point b)
        {
            return (p.X - b.X) * (a.Y - b.Y) - (a.X - b.X) * (p.Y - b.Y);
        }

        private bool PontoDentroTriangulo(Point p, Point a, Point b, Point c)
        {
            float d1 = Sinal(p, a, b);
            float d2 = Sinal(p, b, c);
            float d3 = Sinal(p, c, a);

            bool temNegativo = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool temPositivo = (d1 > 0) || (d2 > 0) || (d3 > 0);

            // Se o ponto ficou do MESMO lado dos tres lados, ele esta dentro.
            return !(temNegativo && temPositivo);
        }

        private int DetectarFace(int mx, int my)
        {
            Point clique = new Point(mx, my);

            for (int i = 0; i < facesVisiveis.Length; i++)
            {
                int f = facesVisiveis[i];
                Point[] p = PontosDaFace(f);   // ja considera escala e translacao

                if (PontoDentroTriangulo(clique, p[0], p[1], p[2]))
                {
                    return f;
                }
            }

            return -1;   // clicou fora do icosaedro
        }

        private void pnlDesenho_MouseClick(object sender, MouseEventArgs e)
        {
            int f = DetectarFace(e.X, e.Y);

            if (f >= 0)
            {
                coresFaces[f] = corSelecionada;
                AtualizarDesenho();
            }
        }

        private void AtualizarDesenho()
        {
            pnlDesenho.Invalidate();
        }

        // ==========================================================
        // EVENTOS DOS CONTROLES
        // ==========================================================

        private void botaoCor_Click(object sender, EventArgs e)
        {
            Button b = (Button)sender;

            corSelecionada = cor(b.BackColor.R, b.BackColor.G, b.BackColor.B);
            lblCorAtual.BackColor = corSelecionada;
        }

        private void tbX_Scroll(object sender, EventArgs e)
        {
            translacaoX = tbX.Value;
            lblX.Text = "Translacao X: " + tbX.Value;
            AtualizarDesenho();
        }

        private void tbY_Scroll(object sender, EventArgs e)
        {
            translacaoY = tbY.Value;
            lblY.Text = "Translacao Y: " + tbY.Value;
            AtualizarDesenho();
        }

        private void tbEscala_Scroll(object sender, EventArgs e)
        {
            escala = tbEscala.Value / 100.0f;
            lblEscala.Text = "Escala: " + escala.ToString("0.00") + "x";
            AtualizarDesenho();
        }

        private void btnLimpar_Click(object sender, EventArgs e)
        {
            InicializarCores();
            lblCorAtual.BackColor = corSelecionada;

            tbX.Value = 0;
            tbY.Value = 0;
            tbEscala.Value = 100;

            translacaoX = 0;
            translacaoY = 0;
            escala = 1.0f;

            lblX.Text = "Translacao X: 0";
            lblY.Text = "Translacao Y: 0";
            lblEscala.Text = "Escala: 1,00x";

            AtualizarDesenho();
        }
    }

    // Painel com DoubleBuffered ligado.
    // Serve so para a figura nao piscar enquanto e redesenhada.
    public class PainelDuplo : Panel
    {
        public PainelDuplo()
        {
            this.DoubleBuffered = true;
        }
    }
}