# Icosaedro 2D

Projeção 2D de um icosaedro em C# com Windows Forms e GDI+, com pintura interativa de faces, translação e escala.

Projeto do 3º bimestre da disciplina de **Introdução à Computação Gráfica** — Colégio UniVap / Fundação Valeparaibana de Ensino.

---

## Sobre o projeto

O icosaedro é um poliedro regular com **12 vértices, 30 arestas e 20 faces triangulares**. Este programa reproduz a projeção 2D desse sólido vista pelo eixo de simetria de ordem 3 (o eixo que passa pelo centro de uma face e sai pelo centro da face oposta).

Como metade das faces aponta para longe do observador, apenas **10 faces ficam visíveis** na projeção. Elas são numeradas de 1 a 10 e podem ser pintadas individualmente.

Todo o desenho é feito com primitivas gráficas básicas, sem engines nem bibliotecas externas.

---

## Funcionalidades

- Geração da projeção 2D a partir dos 12 vértices do icosaedro
- Estrutura interna completa: 30 arestas e 20 faces
- Remoção de arestas ocultas (apenas as 18 arestas visíveis são desenhadas)
- Identificação das 10 faces visíveis com índices de 1 a 10
- Mosaico com 8 cores para pintar as faces individualmente
- Detecção de clique por teste de ponto dentro de triângulo
- Translação nos eixos X e Y por TrackBar
- Escala uniforme de 50% a 200%, aplicada em relação ao centro da figura

---

## Como executar

**Requisitos:** .NET 6.0 ou superior, no Windows.

```bash
git clone https://github.com/vbrisolla/Icosaedro-2D.git
cd Icosaedro-2D
dotnet run --project Projeto3Bi
```

Também é possível abrir o `Projeto3Bi.slnx` diretamente no Visual Studio e executar com F5.

**Uso:** clique em uma cor do mosaico e depois em uma face da figura para pintá-la. Os três controles deslizantes na parte inferior movem e redimensionam o icosaedro. A cor cinza do mosaico devolve a face ao estado original.

---

## A geometria

### Como os 12 vértices se organizam

Vistos pelo eixo de ordem 3, os vértices se distribuem em **4 camadas de 3 vértices**, cada uma formando um triângulo equilátero:

| Camada | Vértices | Raio | Aparece na imagem? |
|---|---|---|---|
| Frente | V1, V2, V3 | 74 | Sim — triângulo interno |
| Meio (frente) | V4, V5, V6 | 120 | Sim — parte do hexágono |
| Meio (fundo) | V7, V8, V9 | 120 | Sim — parte do hexágono |
| Fundo | V10, V11, V12 | 74 | **Não** — ficam ocultos |

As duas camadas do meio, juntas, formam o hexágono externo. As camadas da frente e do fundo formam dois triângulos internos girados 60° entre si, mas apenas o da frente aparece.

Por isso a figura mostra **9 pontos**, e não 12: seis do hexágono e três do triângulo interno frontal.

### Tabela dos vértices

Coordenadas relativas ao centro, em pixels, com Y crescendo para baixo (como na tela).

| Vértice | Índice | Raio | Ângulo | X | Y | Localização |
|---|---|---|---|---|---|---|
| V1 | 0 | 74 | 30° | 64 | −37 | interno, superior direito |
| V2 | 1 | 74 | 150° | −64 | −37 | interno, superior esquerdo |
| V3 | 2 | 74 | 270° | 0 | 74 | interno, inferior (centro) |
| V4 | 3 | 120 | 90° | 0 | −120 | hexágono, topo |
| V5 | 4 | 120 | 30° | 104 | −60 | hexágono, superior direito |
| V6 | 5 | 120 | 330° | 104 | 60 | hexágono, inferior direito |
| V7 | 6 | 120 | 270° | 0 | 120 | hexágono, base |
| V8 | 7 | 120 | 210° | −104 | 60 | hexágono, inferior esquerdo |
| V9 | 8 | 120 | 150° | −104 | −60 | hexágono, superior esquerdo |
| V10 | 9 | 74 | 90° | 0 | −74 | interno traseiro (oculto) |
| V11 | 10 | 74 | 330° | 64 | 37 | interno traseiro (oculto) |
| V12 | 11 | 74 | 210° | −64 | 37 | interno traseiro (oculto) |

### A razão áurea

As coordenadas não são valores fixos digitados no código. Elas são calculadas em tempo de execução por **coordenadas polares**:

```
x = Xc + raio · cos(θ)
y = Yc − raio · sen(θ)
```

com a conversão de graus para radianos `θ = α · π / 180`.

O raio interno não é escolhido arbitrariamente: ele vale `RAIO_EXTERNO / φ`, onde φ é a razão áurea (1,618...). A proporção 74/120 = 0,617 é exatamente 1/φ.

Isso não é coincidência. As coordenadas do icosaedro no espaço 3D são construídas a partir de φ, e essa proporção sobrevive na projeção 2D.

*(O sinal de menos no cálculo de Y existe porque na tela o eixo Y cresce para baixo, ao contrário da trigonometria.)*

### As 10 faces visíveis

Numeradas de cima para baixo, da esquerda para a direita:

| Face | Vértices | Posição |
|---|---|---|
| 1 | 1, 3, 8 | topo esquerdo |
| 2 | 0, 1, 3 | topo centro |
| 3 | 0, 3, 4 | topo direito |
| 4 | 1, 7, 8 | meio, esquerda externa |
| 5 | 0, 1, 2 | **triângulo central** |
| 6 | 0, 4, 5 | meio, direita externa |
| 7 | 1, 2, 7 | meio esquerdo |
| 8 | 0, 2, 5 | meio direito |
| 9 | 2, 6, 7 | base esquerda |
| 10 | 2, 5, 6 | base direita |

### Validação topológica

A fórmula de Euler para poliedros confirma a consistência da estrutura:

```
V − E + F = 2
12 − 30 + 20 = 2  ✓
```

Verificações adicionais que o modelo satisfaz:

- todo vértice tem grau 5 (5 arestas saindo dele)
- toda aresta pertence a exatamente 2 faces
- nenhuma aresta ou face aparece duplicada

---

## Arquitetura

O código separa três responsabilidades que nunca se misturam:

| Camada | Responsabilidade | Onde está |
|---|---|---|
| **Modelo** | a forma pura: vértices, arestas e faces | `vertices`, `arestas`, `faces` |
| **Transformação** | escala e translação aplicadas ao modelo | `TransformarPonto()` |
| **Desenho** | as primitivas do GDI+ que produzem a imagem | `DesenharIcosaedro()` |

O modelo nunca é alterado, apenas lido. Quando o usuário move um controle, o programa não cria uma figura nova: ele reprocessa o mesmo modelo com parâmetros diferentes. Esse é o conceito de transformação geométrica.

### Estrutura de dados

As tabelas de arestas e faces guardam **índices** de vértice, não coordenadas:

```csharp
int[,] arestas = { {0,1}, {0,2}, ... };      // 30 linhas × 2
int[,] faces   = { {1,3,8}, {0,1,3}, ... };  // 20 linhas × 3
```

Essa indireção separa a *topologia* (quem se liga com quem) da *posição* (onde cada ponto está). É o que permite mover ou redimensionar a figura sem tocar nessas tabelas.

### Métodos principais

| Método | Função |
|---|---|
| `InicializarIcosaedro()` | gera os 12 vértices por trigonometria |
| `TransformarPonto()` | aplica escala em relação ao centro e translação |
| `DesenharFaces()` | preenche as 10 faces visíveis |
| `DesenharArestas()` | traça as arestas, com remoção das ocultas |
| `DesenharNumeros()` | escreve os índices 1 a 10 no centro das faces |
| `PontoDentroTriangulo()` | testa se um ponto está dentro de um triângulo |
| `DetectarFace()` | descobre qual face recebeu o clique |

---

## Detalhes de implementação

### Escala em relação ao centro

```csharp
x = CX + (ponto.X - CX) * escala;
```

A conta mede a distância do ponto até o centro, multiplica pela escala e devolve. Usar apenas `x * escala` escalaria em relação à origem da tela (canto superior esquerdo), fazendo a figura fugir para o canto inferior direito enquanto cresce, em vez de crescer parada no lugar.

### Remoção de arestas ocultas

Das 30 arestas do modelo, 12 pertencem exclusivamente a faces que apontam para trás. Desenhar todas produziria uma estrela de seis pontas, diferente da projeção real. Cada aresta só é traçada se pertencer a alguma das 10 faces visíveis, sobrando **18 arestas desenhadas**.

Esse número confere pela fórmula de Euler aplicada a uma figura plana: com 9 pontos visíveis e 10 triângulos, `9 − E + 10 = 1`, logo `E = 18`.

### Detecção de clique

O teste usa o **produto vetorial** para descobrir de que lado de cada aresta o ponto está. Se o clique cai dentro do triângulo, ele fica do mesmo lado dos três lados e os três sinais coincidem. Se cai fora, pelo menos um sinal diverge.

A detecção usa os vértices **já transformados**, os mesmos que foram desenhados. Sem isso, a área clicável ficaria dessincronizada do desenho assim que a figura fosse movida.

### Paint e Invalidate

Todo o desenho acontece no evento `Paint`. Quando algo muda, o programa chama `Invalidate()`, que marca a área como inválida e faz o Windows disparar o `Paint`.

`CreateGraphics()` não é usado porque o desenho seria apagado assim que a janela fosse coberta ou minimizada, já que nada o reconstruiria. Com `Paint`, o sistema sempre sabe redesenhar tudo a partir dos arrays do modelo.

O `PictureBox` já vem com double buffer ativado, o que evita que a figura pisque durante o arrasto dos controles.

---

## Primitivas utilizadas

O projeto usa as primitivas trabalhadas nos exercícios e exemplos de aula:

| Primitiva | Uso |
|---|---|
| `cor(r, g, b)` | criação de cores via `Color.FromArgb` |
| `caneta(r, g, b)` | caneta com espessura padrão |
| `caneta(r, g, b, espessura)` | sobrecarga com espessura definida |
| `pintaLinha(...)` | traçado das arestas via `DrawLine` |
| `Poligono(x, y)` | montagem do `Point[]` de cada face |
| `PreenchePoligono(...)` | preenchimento via `FillPolygon` |
| `preen_Area(cor)` | criação do `SolidBrush` |
| `pintaTexto(...)` | numeração das faces via `DrawString` |

**Adaptação:** `pintaTexto()` não constava na lista original de primitivas, mas foi necessária para cumprir o requisito de identificar as faces com índices de 1 a 10. Ela segue o mesmo padrão das demais (recebe o `PaintEventArgs` e repassa ao `Graphics`) e usa `DrawString` e `Font`, apresentados no material do 1º bimestre.

**Primitivas não utilizadas e o porquê:**

- `HatchBrush` e `TextureBrush` — `SolidBrush` resolve o preenchimento das faces
- caneta tracejada — todas as arestas da projeção são sólidas
- `dElipse` e `pintaPonto` — a figura contém apenas segmentos de reta

---

## Estrutura de arquivos

```
Icosaedro-2D/
├── Projeto3Bi.slnx           solução
└── Projeto3Bi/
    ├── Projeto3Bi.csproj     projeto .NET
    ├── Program.cs            ponto de entrada
    ├── Form1.cs              todo o código do projeto
    ├── Form1.Designer.cs     código gerado pelo designer
    └── Form1.resx            recursos do formulário
```

---

## Tecnologias

- C#
- .NET / Windows Forms
- GDI+ (`System.Drawing`)

Sem bibliotecas externas, engines gráficas ou frameworks de terceiros.
