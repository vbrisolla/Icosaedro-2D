using System;
using System.Drawing;
using System.Windows.Forms;

namespace Projeto3Bi
{
    public partial class Form1 : Form
    {
        PictureBox pictureBox1;
        TrackBar trackBarTransX;
        TrackBar trackBarTransY;
        TrackBar trackBarEscala;
        Label lblTransX;
        Label lblTransY;
        Label lblEscala;

        const int CENTRO_X = 430;
        const int CENTRO_Y = 280;

        // vertices de modelo (indices: 0=A 1=B 2=C  3=V3 4=V6 5=V10 6=V2 7=V7 8=V11)
        int[] modeloX = new int[9] { -78, -48, 126, 78, 204, 126, -78, -204, -126 };
        int[] modeloY = new int[9] { 100, -118, 17, 190, 28, -163, -190, -28, 163 };

        // faces (triangulos) - cada linha guarda os 3 indices de vertice da face
        int[,] faces = new int[10, 3]
        {
            {0,1,2}, {0,2,3}, {0,3,8}, {0,8,7}, {0,7,1},
            {1,7,6}, {1,6,5}, {1,5,2}, {2,3,4}, {2,4,5}
        };

        Color[] corFace = new Color[10];
        Color[] paleta = new Color[6];
        Color corSelecionada;

        int[] finalX = new int[9];
        int[] finalY = new int[9];

        public Form1()
        {
            ConfigurarJanela();
            CriarControles();
            InicializarCores();
            corSelecionada = paleta[0];
        }

        void ConfigurarJanela()
        {
            Text = "Projeto 3 Bimestre - Icosaedro 2D";
            Width = 900;
            Height = 950;
            StartPosition = FormStartPosition.CenterScreen;
        }

        void CriarControles()
        {
            pictureBox1 = new PictureBox();
            pictureBox1.Location = new Point(10, 10);
            pictureBox1.Size = new Size(860, 580);
            pictureBox1.BorderStyle = BorderStyle.FixedSingle;
            pictureBox1.BackColor = Color.White;
            pictureBox1.Paint += PictureBox1_Paint;
            pictureBox1.MouseClick += PictureBox1_MouseClick;
            Controls.Add(pictureBox1);

            // espaço reservado para cada bloco (rótulo + trackbar), bem folgado
            const int alturaBloco = 95;
            int y = 610;

            lblTransX = new Label();
            lblTransX.Text = "Translação X";
            lblTransX.AutoSize = true;
            lblTransX.Location = new Point(30, y);
            Controls.Add(lblTransX);

            trackBarTransX = new TrackBar();
            trackBarTransX.Location = new Point(30, y + 25);
            trackBarTransX.Width = 780;
            trackBarTransX.Minimum = -150;
            trackBarTransX.Maximum = 150;
            trackBarTransX.Value = 0;
            trackBarTransX.TickFrequency = 10;
            trackBarTransX.ValueChanged += (s, e) => pictureBox1.Invalidate();
            Controls.Add(trackBarTransX);

            y += alturaBloco;

            lblTransY = new Label();
            lblTransY.Text = "Translação Y";
            lblTransY.AutoSize = true;
            lblTransY.Location = new Point(30, y);
            Controls.Add(lblTransY);

            trackBarTransY = new TrackBar();
            trackBarTransY.Location = new Point(30, y + 25);
            trackBarTransY.Width = 780;
            trackBarTransY.Minimum = -150;
            trackBarTransY.Maximum = 150;
            trackBarTransY.Value = 0;
            trackBarTransY.TickFrequency = 10;
            trackBarTransY.ValueChanged += (s, e) => pictureBox1.Invalidate();
            Controls.Add(trackBarTransY);

            y += alturaBloco;

            lblEscala = new Label();
            lblEscala.Text = "Escala (%)";
            lblEscala.AutoSize = true;
            lblEscala.Location = new Point(30, y);
            Controls.Add(lblEscala);

            trackBarEscala = new TrackBar();
            trackBarEscala.Location = new Point(30, y + 25);
            trackBarEscala.Width = 780;
            trackBarEscala.Minimum = 50;
            trackBarEscala.Maximum = 200;
            trackBarEscala.Value = 100;
            trackBarEscala.TickFrequency = 10;
            trackBarEscala.ValueChanged += (s, e) => pictureBox1.Invalidate();
            Controls.Add(trackBarEscala);
        }

        // -------- funções de primitivas (mesmo estilo do projeto anterior) --------
        public Color CriaCor(int r, int g, int b)
        {
            return Color.FromArgb(r, g, b);
        }

        public Pen CriaCaneta(Color cor, float espessura = 1)
        {
            return new Pen(cor, espessura);
        }

        void InicializarCores()
        {
            paleta[0] = CriaCor(220, 40, 40);
            paleta[1] = CriaCor(40, 120, 220);
            paleta[2] = CriaCor(240, 200, 30);
            paleta[3] = CriaCor(40, 180, 90);
            paleta[4] = CriaCor(160, 60, 200);
            paleta[5] = CriaCor(255, 255, 255);

            for (int i = 0; i < 10; i++)
            {
                corFace[i] = CriaCor(210, 210, 210);
            }
        }

        void AtualizarVerticesFinais()
        {
            int escala = trackBarEscala.Value;
            int transX = trackBarTransX.Value;
            int transY = trackBarTransY.Value;

            for (int i = 0; i < 9; i++)
            {
                finalX[i] = modeloX[i] * escala / 100 + transX + CENTRO_X;
                finalY[i] = modeloY[i] * escala / 100 + transY + CENTRO_Y;
            }
        }

        // área (com sinal) do triângulo p1-p2-p3 : usado no teste ponto-dentro-do-triângulo
        int Area2(int x1, int y1, int x2, int y2, int x3, int y3)
        {
            return (x2 - x1) * (y3 - y1) - (x3 - x1) * (y2 - y1);
        }

        bool PontoDentroTriangulo(int px, int py, int ax, int ay, int bx, int by, int cx, int cy)
        {
            int d1 = Area2(px, py, ax, ay, bx, by);
            int d2 = Area2(px, py, bx, by, cx, cy);
            int d3 = Area2(px, py, cx, cy, ax, ay);

            bool temNegativo = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool temPositivo = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(temNegativo && temPositivo);
        }

        int FaceClicada(int px, int py)
        {
            for (int i = 0; i < 10; i++)
            {
                int v1 = faces[i, 0];
                int v2 = faces[i, 1];
                int v3 = faces[i, 2];

                if (PontoDentroTriangulo(px, py, finalX[v1], finalY[v1], finalX[v2], finalY[v2], finalX[v3], finalY[v3]))
                {
                    return i;
                }
            }
            return -1;
        }

        void DesenharIcosaedro(Graphics gfx)
        {
            for (int i = 0; i < 10; i++)
            {
                int v1 = faces[i, 0];
                int v2 = faces[i, 1];
                int v3 = faces[i, 2];

                Point[] pontos = new Point[]
                {
                    new Point(finalX[v1], finalY[v1]),
                    new Point(finalX[v2], finalY[v2]),
                    new Point(finalX[v3], finalY[v3])
                };

                using (SolidBrush pincel = new SolidBrush(corFace[i]))
                {
                    gfx.FillPolygon(pincel, pontos);
                }

                using (Pen caneta = CriaCaneta(Color.Black, 1))
                {
                    gfx.DrawPolygon(caneta, pontos);
                }
            }
        }

        void DesenharPaleta(Graphics gfx)
        {
            for (int i = 0; i < 6; i++)
            {
                int px = 30 + i * 45;
                int py = 15;

                using (SolidBrush pincel = new SolidBrush(paleta[i]))
                {
                    gfx.FillRectangle(pincel, px, py, 35, 35);
                }

                if (paleta[i] == corSelecionada)
                {
                    using (Pen caneta = CriaCaneta(Color.Black, 2))
                    {
                        gfx.DrawRectangle(caneta, px - 3, py - 3, 41, 41);
                    }
                }
            }
        }

        void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            AtualizarVerticesFinais();
            DesenharIcosaedro(e.Graphics);
            DesenharPaleta(e.Graphics);
        }

        void PictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            // clique na paleta de cores?
            for (int i = 0; i < 6; i++)
            {
                int px = 30 + i * 45;
                int py = 15;

                if (e.X >= px && e.X <= px + 35 && e.Y >= py && e.Y <= py + 35)
                {
                    corSelecionada = paleta[i];
                    pictureBox1.Invalidate();
                    return;
                }
            }

            // clique em uma face do icosaedro?
            int faceEscolhida = FaceClicada(e.X, e.Y);
            if (faceEscolhida >= 0)
            {
                corFace[faceEscolhida] = corSelecionada;
                pictureBox1.Invalidate();
            }
        }
    }
}
