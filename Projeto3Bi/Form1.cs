// ============================================================================
// Projeto 3o Bimestre - Introducao a Computacao Grafica
// Projecao 2D de um icosaedro com mosaico de cores e transformacoes geometricas
// ============================================================================

using System;                    // tipos basicos da linguagem
using System.Drawing;            // GDI+: Color, Pen, SolidBrush, Point, Size, Graphics
using System.Windows.Forms;      // controles: Form, PictureBox, TrackBar, Label, eventos de mouse

namespace Projeto3Bi
{
    
    public partial class Form1 : Form
    {
        // ------------------------------------------------------------------
        // CONTROLES DA INTERFACE
        // Declarados aqui (nivel de classe) e nao dentro de um metodo, porque
        // precisam ser acessados de varios pontos do codigo. Neste momento
        // ainda valem null; sao instanciados em CriarControles().
        // ------------------------------------------------------------------
        PictureBox pictureBox1;      // area de desenho onde o icosaedro e a paleta aparecem
        TrackBar trackBarTransX;     // slider de translacao horizontal
        TrackBar trackBarTransY;     // slider de translacao vertical
        TrackBar trackBarEscala;     // slider de escala (em porcentagem)
        Label lblTransX;             // rotulo de texto do slider de translacao X
        Label lblTransY;             // rotulo de texto do slider de translacao Y
        Label lblEscala;             // rotulo de texto do slider de escala

        // ------------------------------------------------------------------
        // CENTRO DA AREA DE DESENHO
        // O modelo do icosaedro e construido em torno da origem (0,0). Estas
        // constantes deslocam a figura para o meio do PictureBox (860x580).
        // "const" = valor fixo resolvido em tempo de compilacao.
        // ------------------------------------------------------------------
        const int CENTRO_X = 430;
        const int CENTRO_Y = 280;

        // ------------------------------------------------------------------
        // VERTICES DO MODELO
        // Dois vetores paralelos: o vertice i e o par (modeloX[i], modeloY[i]).
        // Mesma tecnica do slide "Exemplo da criacao de Poligono" do material.
        //
        // ATENCAO: estas coordenadas sao RELATIVAS AO CENTRO DA FIGURA, nao a
        // tela. A origem (0,0) e o centro do icosaedro. Em Y, negativo = acima
        // do centro e positivo = abaixo (na tela o eixo Y cresce para baixo).
        //
        // Sao 9 pontos porque, na projecao 2D do icosaedro, os 12 vertices do
        // solido se sobrepoem: 6 formam o hexagono externo e 3 o triangulo interno.
        // ------------------------------------------------------------------
        // vertices de modelo (indices: 0=A 1=B 2=C  3=V3 4=V6 5=V10 6=V2 7=V7 8=V11)
        int[] modeloX = new int[9] { -78, -48, 126, 78, 204, 126, -78, -204, -126 };
        int[] modeloY = new int[9] { 100, -118, 17, 190, 28, -163, -190, -28, 163 };

        // ------------------------------------------------------------------
        // TABELA DE FACES (topologia da malha)
        // Matriz retangular 10x3: cada linha e uma face triangular e guarda os
        // INDICES dos seus 3 vertices, nunca as coordenadas. Essa indirecao e o
        // que permite trocar completamente modeloX/modeloY sem mexer aqui, ja
        // que as ligacoes entre os vertices continuam as mesmas.
        //
        // Sao 10 faces porque na projecao so metade das 20 faces do icosaedro
        // fica visivel. A face {0,1,2} e o triangulo interno central.
        // Cada vertice interno (0, 1 e 2) aparece em 5 faces, o que confere com
        // a geometria do solido (todo vertice tem grau 5).
        // ------------------------------------------------------------------
        // faces (triangulos) - cada linha guarda os 3 indices de vertice da face
        int[,] faces = new int[10, 3]
        {
            {0,1,2}, {0,2,3}, {0,3,8}, {0,8,7}, {0,7,1},
            {1,7,6}, {1,6,5}, {1,5,2}, {2,3,4}, {2,4,5}
        };

        // ------------------------------------------------------------------
        // ESTADO DE CORES
        // ------------------------------------------------------------------
        Color[] corFace = new Color[10];   // cor atual de cada uma das 10 faces (o "estado da pintura")
        Color[] paleta = new Color[6];     // as 6 cores do mosaico de selecao
        Color corSelecionada;              // cor ativa: a que sera aplicada no proximo clique numa face

        // ------------------------------------------------------------------
        // VERTICES JA TRANSFORMADOS
        // Os mesmos 9 pontos, mas com escala, translacao e centralizacao
        // aplicadas, prontos em coordenadas de tela. Sao recalculados a cada
        // repintura e servem para DOIS fins ao mesmo tempo: desenhar a figura e
        // testar os cliques. Como os dois usam a mesma fonte, o clique sempre
        // bate com o que esta desenhado, mesmo com a figura ampliada ou movida.
        // ------------------------------------------------------------------
        int[] finalX = new int[9];
        int[] finalY = new int[9];

        // ------------------------------------------------------------------
        // CONSTRUTOR
        // Roda uma unica vez, quando Program.cs executa Application.Run(new Form1()).
        // A ordem importa: CriarControles() precisa vir antes de qualquer uso de
        // pictureBox1, e corSelecionada so pode ser definida depois que a paleta
        // ja foi preenchida.
        // ------------------------------------------------------------------
        public Form1()
        {
            ConfigurarJanela();
            CriarControles();
            InicializarCores();
            corSelecionada = paleta[0];   // comeca com o vermelho ativo, para nunca haver estado indefinido
        }

        // ------------------------------------------------------------------
        // Propriedades herdadas de Form, por isso escritas sem prefixo
        // (equivale a this.Text = ..., this.Width = ..., etc.)
        // ------------------------------------------------------------------
        void ConfigurarJanela()
        {
            Text = "Projeto 3 Bimestre - Icosaedro 2D";        // titulo na barra da janela
            Width = 900;                                        // largura EXTERNA (inclui bordas)
            Height = 950;                                       // altura EXTERNA (inclui barra de titulo)
            StartPosition = FormStartPosition.CenterScreen;      // abre no meio do monitor
        }

        // ------------------------------------------------------------------
        // MONTAGEM DA INTERFACE
        // Tudo e criado por codigo, sem usar o designer visual.
        // ------------------------------------------------------------------
        void CriarControles()
        {
            // ---- area de desenho ----
            pictureBox1 = new PictureBox();                        // instancia o controle
            pictureBox1.Location = new Point(10, 10);              // canto superior esquerdo, a 10px da borda do form
            pictureBox1.Size = new Size(860, 580);                 // dimensoes da area util de desenho
            pictureBox1.BorderStyle = BorderStyle.FixedSingle;     // moldura fina de 1px, delimita a area
            pictureBox1.BackColor = Color.White;                   // fundo branco

            // O operador += em um evento INSCREVE um metodo na lista de quem sera
            // avisado. Nao e soma. Estes metodos nunca sao chamados diretamente no
            // codigo: quem os chama e o Windows, quando o evento acontece.
            pictureBox1.Paint += PictureBox1_Paint;                // "quando precisar redesenhar, chame este metodo"
            pictureBox1.MouseClick += PictureBox1_MouseClick;      // "quando houver clique, chame este metodo"

            Controls.Add(pictureBox1);                             // insere na janela; sem isto o controle nao aparece

            // ---- layout vertical dos sliders ----
            // "y" funciona como um cursor que desce a cada bloco criado. Comeca
            // logo abaixo do PictureBox, que termina em 10 + 580 = 590.
            // espaço reservado para cada bloco (rótulo + trackbar), bem folgado
            const int alturaBloco = 95;
            int y = 610;

            // ---- BLOCO 1: translacao horizontal ----
            lblTransX = new Label();
            lblTransX.Text = "Translação X";
            lblTransX.AutoSize = true;                             // o Label se ajusta sozinho ao tamanho do texto
            lblTransX.Location = new Point(30, y);
            Controls.Add(lblTransX);

            trackBarTransX = new TrackBar();
            trackBarTransX.Location = new Point(30, y + 25);        // 25px abaixo do rotulo, para nao sobrepor
            trackBarTransX.Width = 780;
            trackBarTransX.Minimum = -150;                          // desloca ate 150px para a esquerda
            trackBarTransX.Maximum = 150;                           // desloca ate 150px para a direita
            trackBarTransX.Value = 0;                               // posicao inicial: nenhum deslocamento
            trackBarTransX.TickFrequency = 10;                      // um risquinho de marcacao a cada 10 unidades

            // Expressao lambda: metodo anonimo criado na hora. (s, e) sao o objeto
            // que disparou e os argumentos do evento; nao sao usados, mas precisam
            // existir para a assinatura bater com o delegate EventHandler.
            // Invalidate() marca a area como "suja" e faz o Windows disparar o
            // evento Paint. E o Invalidate() do primeiro slide do material.
            trackBarTransX.ValueChanged += (s, e) => pictureBox1.Invalidate();
            Controls.Add(trackBarTransX);

            y += alturaBloco;                                       // desce o cursor para o proximo bloco

            // ---- BLOCO 2: translacao vertical (estrutura identica ao bloco 1) ----
            lblTransY = new Label();
            lblTransY.Text = "Translação Y";
            lblTransY.AutoSize = true;
            lblTransY.Location = new Point(30, y);
            Controls.Add(lblTransY);

            trackBarTransY = new TrackBar();
            trackBarTransY.Location = new Point(30, y + 25);
            trackBarTransY.Width = 780;
            trackBarTransY.Minimum = -150;                          // desloca ate 150px para cima
            trackBarTransY.Maximum = 150;                           // desloca ate 150px para baixo
            trackBarTransY.Value = 0;
            trackBarTransY.TickFrequency = 10;
            trackBarTransY.ValueChanged += (s, e) => pictureBox1.Invalidate();
            Controls.Add(trackBarTransY);

            y += alturaBloco;

            // ---- BLOCO 3: escala ----
            lblEscala = new Label();
            lblEscala.Text = "Escala (%)";
            lblEscala.AutoSize = true;
            lblEscala.Location = new Point(30, y);
            Controls.Add(lblEscala);

            trackBarEscala = new TrackBar();
            trackBarEscala.Location = new Point(30, y + 25);
            trackBarEscala.Width = 780;
            // Faixa em PORCENTAGEM: trabalhar com inteiros evita precisar de float,
            // que o TrackBar nao aceita. 100 = tamanho original.
            trackBarEscala.Minimum = 50;                            // reduz ate a metade
            trackBarEscala.Maximum = 200;                           // amplia ate o dobro
            trackBarEscala.Value = 100;
            trackBarEscala.TickFrequency = 10;
            trackBarEscala.ValueChanged += (s, e) => pictureBox1.Invalidate();
            Controls.Add(trackBarEscala);
        }

        // -------- funções de primitivas (mesmo estilo do projeto anterior) --------

        // Embrulho de Color.FromArgb(r, g, b): monta uma cor a partir dos tres
        // canais RGB (0 a 255 cada), exatamente como nos exemplos do material.
        public Color CriaCor(int r, int g, int b)
        {
            return Color.FromArgb(r, g, b);
        }

        // Embrulho de new Pen(cor, espessura), o construtor do slide "Espessura
        // de uma linha". "float espessura = 1" e um parametro opcional: se a
        // chamada omitir o segundo argumento, o compilador preenche 1.
        public Pen CriaCaneta(Color cor, float espessura = 1)
        {
            return new Pen(cor, espessura);
        }

        // ------------------------------------------------------------------
        // ESTADO INICIAL DAS CORES
        // ------------------------------------------------------------------
        void InicializarCores()
        {
            paleta[0] = CriaCor(220, 40, 40);      // vermelho
            paleta[1] = CriaCor(40, 120, 220);     // azul
            paleta[2] = CriaCor(240, 200, 30);     // amarelo
            paleta[3] = CriaCor(40, 180, 90);      // verde
            paleta[4] = CriaCor(160, 60, 200);     // roxo
            paleta[5] = CriaCor(255, 255, 255);    // branco: na pratica funciona como borracha

            // Todas as faces comecam em cinza claro, para a figura ja aparecer
            // preenchida assim que o programa abre.
            for (int i = 0; i < 10; i++)
            {
                corFace[i] = CriaCor(210, 210, 210);
            }
        }

        // ------------------------------------------------------------------
        // PIPELINE DE TRANSFORMACAO GEOMETRICA
        // Le os tres sliders e aplica escala + translacao nos 9 vertices,
        // gravando o resultado em finalX/finalY. O modelo original NUNCA e
        // alterado, apenas lido.
        //
        // A ORDEM DAS OPERACOES E O QUE FAZ FUNCIONAR:
        //
        //  1) ESCALA  ->  x' = x * sx   (formula do material)
        //     Como as coordenadas de modelo estao centradas na origem,
        //     multiplicar por um fator estica a figura a partir do PROPRIO
        //     centro dela. Se a escala fosse aplicada depois de somar CENTRO_X,
        //     a figura fugiria para o canto inferior direito enquanto cresce,
        //     porque estaria escalando em relacao ao ponto (0,0) da janela.
        //
        //  2) TRANSLACAO  ->  x' = x + tx   (formula do material)
        //     O vetor de deslocamento controlado pelo usuario.
        //
        //  3) CENTRALIZACAO  ->  + CENTRO_X
        //     Translacao fixa que leva a figura para o meio do PictureBox.
        //     Existe porque a tela tem origem no canto superior esquerdo,
        //     enquanto o modelo tem origem no centro.
        //
        // Detalhe da conta: a multiplicacao vem ANTES da divisao por 100. Se
        // fosse "escala / 100" primeiro, a divisao inteira daria 0 ou 1 e a
        // escala quebraria. Do jeito que esta, a divisao apenas trunca fracoes
        // menores que 1 pixel, o que e irrelevante aqui.
        // ------------------------------------------------------------------
        void AtualizarVerticesFinais()
        {
            int escala = trackBarEscala.Value;      // le a porcentagem atual
            int transX = trackBarTransX.Value;      // le o deslocamento horizontal atual
            int transY = trackBarTransY.Value;      // le o deslocamento vertical atual

            for (int i = 0; i < 9; i++)
            {
                finalX[i] = modeloX[i] * escala / 100 + transX + CENTRO_X;
                finalY[i] = modeloY[i] * escala / 100 + transY + CENTRO_Y;
            }
        }

        // ------------------------------------------------------------------
        // PRODUTO VETORIAL (cross product) de dois vetores que saem do ponto 1:
        // um vai ate o ponto 2, o outro ate o ponto 3.
        //   u = (x2-x1, y2-y1)   v = (x3-x1, y3-y1)
        //   resultado = u.x * v.y - v.x * u.y
        //
        // O valor absoluto e o DOBRO da area do triangulo (dai o nome). Mas o
        // que interessa aqui e o SINAL: ele diz de que lado da reta que passa
        // por 1 e 2 esta o ponto 3. Zero significa que os tres sao colineares.
        // ------------------------------------------------------------------
        // área (com sinal) do triângulo p1-p2-p3 : usado no teste ponto-dentro-do-triângulo
        int Area2(int x1, int y1, int x2, int y2, int x3, int y3)
        {
            return (x2 - x1) * (y3 - y1) - (x3 - x1) * (y2 - y1);
        }

        // ------------------------------------------------------------------
        // TESTE PONTO-DENTRO-DO-TRIANGULO
        // O ponto testado e (px, py); o triangulo tem vertices A, B e C.
        //
        // Logica geometrica: se o ponto esta DENTRO, ele fica do mesmo lado dos
        // tres segmentos, entao os tres sinais dao iguais. Se esta FORA, pelo
        // menos um sinal diverge dos outros.
        //
        // "temNegativo && temPositivo" e verdadeiro quando ha MISTURA de sinais,
        // ou seja, quando o ponto esta fora. O "!" na frente inverte o resultado.
        //
        // Valores zero nao entram em nenhuma das duas variaveis, entao um clique
        // exatamente sobre uma aresta conta como dentro. Isso evita "buracos"
        // nas bordas onde nenhuma face responderia ao clique.
        // ------------------------------------------------------------------
        bool PontoDentroTriangulo(int px, int py, int ax, int ay, int bx, int by, int cx, int cy)
        {
            int d1 = Area2(px, py, ax, ay, bx, by);   // sinal do ponto em relacao ao lado A-B
            int d2 = Area2(px, py, bx, by, cx, cy);   // sinal do ponto em relacao ao lado B-C
            int d3 = Area2(px, py, cx, cy, ax, ay);   // sinal do ponto em relacao ao lado C-A

            bool temNegativo = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool temPositivo = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(temNegativo && temPositivo);
        }

        // ------------------------------------------------------------------
        // DESCOBRE QUAL FACE FOI CLICADA
        // Percorre as 10 faces, le os 3 indices de cada uma na tabela e usa
        // esses indices para buscar as coordenadas TRANSFORMADAS em finalX/finalY.
        // E essa busca indireta que faz a deteccao acompanhar a figura quando
        // ela e escalada ou movida pelos sliders.
        //
        // Retorna o indice da primeira face que contem o ponto (como as faces
        // nao se sobrepoem, a primeira encontrada e a correta), ou -1 se o
        // clique caiu fora da figura.
        // ------------------------------------------------------------------
        int FaceClicada(int px, int py)
        {
            for (int i = 0; i < 10; i++)
            {
                int v1 = faces[i, 0];   // indice do 1o vertice da face i
                int v2 = faces[i, 1];   // indice do 2o vertice da face i
                int v3 = faces[i, 2];   // indice do 3o vertice da face i

                if (PontoDentroTriangulo(px, py, finalX[v1], finalY[v1], finalX[v2], finalY[v2], finalX[v3], finalY[v3]))
                {
                    return i;
                }
            }
            return -1;   // convencao de "nenhuma face encontrada"
        }

        // ------------------------------------------------------------------
        // DESENHO DA FIGURA
        // Recebe o contexto grafico (o mesmo e.Graphics dos exemplos de aula).
        // Para cada face: le os 3 indices, monta o Point[] com as coordenadas
        // finais e chama as primitivas de preenchimento e contorno.
        // ------------------------------------------------------------------
        void DesenharIcosaedro(Graphics gfx)
        {
            for (int i = 0; i < 10; i++)
            {
                int v1 = faces[i, 0];
                int v2 = faces[i, 1];
                int v3 = faces[i, 2];

                // Monta o vetor de pontos do triangulo. Mesmo padrao do slide
                // "Exemplo da criacao de Poligono", que preenche um Point[] a
                // partir dos vetores de coordenadas.
                Point[] pontos = new Point[]
                {
                    new Point(finalX[v1], finalY[v1]),
                    new Point(finalX[v2], finalY[v2]),
                    new Point(finalX[v3], finalY[v3])
                };

                // O bloco "using" garante que Dispose() seja chamado ao sair do
                // escopo, liberando o recurso grafico. Isso importa porque o
                // Paint roda dezenas de vezes por segundo enquanto o usuario
                // arrasta um slider; sem isso, milhares de objetos GDI+ ficariam
                // acumulados sem liberacao.

                // 1) PREENCHIMENTO com a cor guardada para esta face
                using (SolidBrush pincel = new SolidBrush(corFace[i]))
                {
                    gfx.FillPolygon(pincel, pontos);
                }

                // 2) CONTORNO por cima. A ordem e obrigatoria: se o contorno
                //    viesse antes, o preenchimento passaria por cima e o apagaria.
                //    Como cada triangulo desenha a propria borda, todas as arestas
                //    internas da malha ficam visiveis, que e o efeito da imagem
                //    de referencia. Arestas compartilhadas sao tracadas duas
                //    vezes, uma sobre a outra, o que e inofensivo.
                using (Pen caneta = CriaCaneta(Color.Black, 1))
                {
                    gfx.DrawPolygon(caneta, pontos);
                }
            }
        }

        // ------------------------------------------------------------------
        // DESENHO DO MOSAICO DE CORES
        // Seis quadrados de 35x35 lado a lado, no topo da area de desenho.
        // ------------------------------------------------------------------
        void DesenharPaleta(Graphics gfx)
        {
            for (int i = 0; i < 6; i++)
            {
                int px = 30 + i * 45;   // passo de 45 para quadrados de 35 => 10px de espaco entre eles
                int py = 15;            // todos na mesma linha horizontal

                // quadrado solido com a cor da paleta
                using (SolidBrush pincel = new SolidBrush(paleta[i]))
                {
                    gfx.FillRectangle(pincel, px, py, 35, 35);
                }

                // Marca visualmente qual cor esta ativa. A comparacao funciona
                // porque Color e um struct comparado por valor.
                if (paleta[i] == corSelecionada)
                {
                    using (Pen caneta = CriaCaneta(Color.Black, 2))
                    {
                        // recua 3px em cada lado e cresce 6 no total (35+3+3=41),
                        // entao a moldura fica POR FORA do quadrado sem cobri-lo
                        gfx.DrawRectangle(caneta, px - 3, py - 3, 41, 41);
                    }
                }
            }
        }

        // ------------------------------------------------------------------
        // EVENTO PAINT - o unico lugar onde o desenho realmente acontece
        //
        // A assinatura (object sender, PaintEventArgs e) e obrigatoria, definida
        // pelo delegate do evento. O "e.Graphics" e o contexto de desenho
        // fornecido pelo Windows, valido SOMENTE durante esta chamada; por isso
        // todo o desenho tem que ocorrer aqui dentro.
        //
        // Este metodo roda sempre que a janela e redimensionada, sai de tras de
        // outra janela, ou alguem chama Invalidate().
        // ------------------------------------------------------------------
        void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            AtualizarVerticesFinais();      // 1) recalcula os vertices a partir dos sliders
            DesenharIcosaedro(e.Graphics);  // 2) desenha as 10 faces
            DesenharPaleta(e.Graphics);     // 3) desenha o mosaico de cores por cima
        }

        // ------------------------------------------------------------------
        // EVENTO MOUSECLICK
        // MouseEventArgs e a classe do slide "Classe MouseEventArgs" do material.
        // e.X e e.Y trazem a posicao do clique RELATIVA AO PICTUREBOX (nao ao
        // formulario nem a tela). Como o desenho tambem usa coordenadas do
        // PictureBox, os dois batem sem precisar de conversao.
        //
        // Importante: o clique NAO desenha nada. Ele apenas altera um valor em
        // memoria e pede uma repintura. O desenho continua sendo exclusividade
        // do Paint. E essa disciplina que mantem a tela sempre consistente com o
        // estado do programa (se a janela for minimizada e restaurada, tudo e
        // redesenhado corretamente a partir dos arrays, sem perder as cores).
        // ------------------------------------------------------------------
        void PictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            // ETAPA 1: o clique caiu em algum quadrado da paleta?
            // Teste de retangulo alinhado aos eixos: basta comparar X e Y com os
            // limites, bem mais simples que o teste de triangulo.
            // clique na paleta de cores?
            for (int i = 0; i < 6; i++)
            {
                int px = 30 + i * 45;   // mesmas contas de DesenharPaleta, para as areas coincidirem
                int py = 15;

                if (e.X >= px && e.X <= px + 35 && e.Y >= py && e.Y <= py + 35)
                {
                    corSelecionada = paleta[i];   // troca a cor ativa
                    pictureBox1.Invalidate();     // redesenha para atualizar a moldura de selecao
                    return;                       // encerra aqui; sem este return o codigo seguiria e tentaria pintar uma face
                }
            }

            // ETAPA 2: o clique caiu em alguma face do icosaedro?
            // clique em uma face do icosaedro?
            int faceEscolhida = FaceClicada(e.X, e.Y);
            if (faceEscolhida >= 0)               // lembrando que -1 significa "nenhuma face"
            {
                corFace[faceEscolhida] = corSelecionada;   // grava a cor ativa naquela face
                pictureBox1.Invalidate();                  // pede a repintura
            }
        }
    }
}