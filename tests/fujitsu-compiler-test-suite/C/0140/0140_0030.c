#include <stdio.h>


#define N 100
#define ANS 7106
int main()
{
  int i, j;
  int a[N][N];
  int sum = 0;
  int v, w;

  for (i = 0; i < N; i++) {
    for (j = 0; j < N; j++) {
	a[i][j] = i*j % 4;
    }
  }

  v = a[34][15]*a[48][22] % 3;
  w = a[17][33]*a[27][42] % 3;


  for (i = 15; i < N - 15; i++) {
    a[i][0] = v * w;
    a[i][1] = j + w;
    a[i][3] = v * w;
    a[i-1][4] = v * w;
    a[i][5] = j + w;
    a[i][6] = v * w;
    a[i][7] = v * w;
    a[i][8] = v * w;
    a[i][9] = v * w;
    a[i][10] = v * w;
    a[i][j-9] = j + w;
    a[i][j-8] = v * w;
    a[i][j-7] = j + w;
    a[i][j-6] = v * w;
    a[i-2][j-5] = j + w;
    a[i][j-4] = v * w;
    a[i][j-3] = j + w;
    a[i][j-2] = v * w;
    a[i][j-1] = j + w;
    a[i][11] = v * w;
    a[i][12] = v * w;
    a[i][13] = j + w;
    a[i][14] = v * w;
    a[i][15] = v * w;
    a[i][16] = v * w;
    a[i][17] = v * w;
    a[i-3][18] = j + w;
    a[i][19] = v * w;
    a[i][20] = v + w;
    a[i][21] = v + w;
    i = i + 3;
    a[i][22] = v + w;
    a[i][23] = j + w;
    a[i][24] = v + w;
    a[i][25] = v + w;
    a[i][26] = v + w;
    a[i][27] = v + w;
    a[i][26] = j + w;
    a[i][27] = v + w;
    a[i][28] = v + w;
    a[i][29] = j - w;
    a[i][30] = v + w;
    a[i][31] = v + w;
    a[i][32] = v + w;
    a[i][33] = v + w;
    a[i][j-11] = v + w;
    a[i][j-10] = v + w;
    a[i][j-9] = j - w;
    a[i][j-8] = v + w;
    a[i][j-7] = j - w;
    a[i][j-6] = v + w;
    a[i][j-5] = j - w;
    a[i][j-4] = v + w;
    a[i][j-3] = j - w;
    a[i][j-2] = v + w;
    a[i][j-1] = j - w;
    a[i][j-3] = v + w;
    a[i][j-4] = v + w;
    a[i][j-5] = v + w;
    a[i][j-35] = j - w;
    a[i][j-34] = j - w;
    a[i][j-15] = v + w;
    a[i][j-16] = v + w;
    a[i][j-17] = v + w;
    a[i][j-18] = j - w;
    a[i][j-19] = v + w;
    a[i][j-20] = v + w;
    a[i][j-21] = v - w;
    a[i][j-22] = v - w;
    a[i][j-23] = j - w;
    a[i][j-24] = v - w;
    i = i - 2;
    a[i][j-25] = v - w;
    a[i][j-26] = v - w;
    a[i][j-27] = v - w;
    a[i][j-26] = j - w;
    a[i][j-27] = v - w;
    a[i][j-28] = v - w;
    a[i][j-29] = j - w;
    a[i][j-30] = v - w;
    a[i][j-31] = v - w;
    a[i][j-32] = v - w;
    a[i][j-33] = v - w;
    a[i][j-11] = v - w;
    a[i][j-10] = v - w;
    a[i][j-9] = j - w;
    a[i][j-8] = v - w;
    a[i][j-7] = i - w;
    a[i][j-6] = v - w;
    a[i][j-5] = i - w;
    a[i][j-4] = v - w;
    a[i][j-3] = i - w;
    a[i][j-2] = v - w;
    a[i][j-1] = i - w;
    a[i][36] = v - w;
    a[i][37] = v - w;
    a[i][38] = v - w;
    a[i][35] = i - w;
    a[i][34] = i - w;
    i = i - 1;
    a[i][j-25] = v - w;
    a[i][j-26] = v - w;
    a[i][j-27] = v - w;
    a[i][j-26] = j - w;
    a[i][j-27] = v - w;
    a[i][j-28] = v - w;
    a[i][j-29] = j - w;
    a[i][j-30] = v - w;
    a[i][j-31] = v - w;
    a[i][j-32] = v - w;
    a[i][j-33] = v - w;
    a[i][j-11] = v - w;
    a[i][j-10] = v - w;
    a[i][j-9] = j - w;
    a[i][j-8] = v - w;
    a[i][j-7] = i - w;
    a[i][j-6] = v - w;
    a[i][j-5] = i - w;
    a[i][j-4] = v - w;
    a[i][j-3] = i - w;
    a[i][j-2] = v - w;
    a[i][j-1] = i - w;
    a[i][36] = v - w;
    a[i][37] = v - w;
    a[i][38] = v - w;
    a[i][35] = i - w;
    a[i][34] = i - w;
    i = i - 5;
    a[i][0] = v * w;
    a[i][1] = j + w;
    a[i][3] = v * w;
    a[i-1][4] = v * w;
    a[i][5] = j + w;
    a[i][6] = v * w;
    a[i][7] = v * w;
    a[i][8] = v * w;
    a[i][9] = v * w;
    a[i][10] = v * w;
    a[i][j-9] = j + w;
    a[i][j-8] = v * w;
    a[i][j-7] = j + w;
    a[i][j-6] = v * w;
    a[i-2][j-5] = j + w;
    a[i][j-4] = v * w;
    a[i][j-3] = j + w;
    a[i][j-2] = v * w;
    a[i][j-1] = j + w;
    a[i][11] = v * w;
    a[i][12] = v * w;
    a[i][13] = j + w;
    a[i][14] = v * w;
    a[i][15] = v * w;
    a[i][16] = v * w;
    a[i][17] = v * w;
    a[i-3][18] = j + w;
    a[i][19] = v * w;
    a[i][20] = v + w;
    a[i][21] = v + w;
    i = i + 3;
    a[i][22] = v + w;
    a[i][23] = j + w;
    a[i][24] = v + w;
    a[i][25] = v + w;
    a[i][26] = v + w;
    a[i][27] = v + w;
    a[i][26] = j + w;
    a[i][27] = v + w;
    a[i][28] = v + w;
    a[i][29] = j - w;
    a[i][30] = v + w;
    a[i][31] = v + w;
    a[i][32] = v + w;
    a[i][33] = v + w;
    a[i][j-11] = v + w;
    a[i][j-10] = v + w;
    a[i][j-9] = j - w;
    a[i][j-8] = v + w;
    a[i][j-7] = j - w;
    a[i][j-6] = v + w;
    a[i][j-5] = j - w;
    a[i][j-4] = v + w;
    a[i][j-3] = j - w;
    a[i][j-2] = v + w;
    a[i][j-1] = j - w;
    a[i][j-3] = v + w;
    a[i][j-4] = v + w;
    a[i][j-5] = v + w;
    a[i][j-35] = j - w;
    a[i][j-34] = j - w;
    a[i][j-15] = v + w;
    a[i][j-16] = v + w;
    a[i][j-17] = v + w;
    a[i][j-18] = j - w;
    a[i][j-19] = v + w;
    a[i][j-20] = v + w;
    a[i][j-21] = v - w;
    a[i][j-22] = v - w;
    a[i][j-23] = j - w;
    a[i][j-24] = v - w;
    i = i + 2;
    a[i][j-25] = v - w;
    a[i][j-26] = v - w;
    a[i][j-27] = v - w;
    a[i][j-26] = j - w;
    a[i][j-27] = v - w;
    a[i][j-28] = v - w;
    a[i][j-29] = j - w;
    a[i][j-30] = v - w;
    a[i][j-31] = v - w;
    a[i][j-32] = v - w;
    a[i][j-33] = v - w;
    a[i][j-11] = v - w;
    a[i][j-10] = v - w;
    a[i][j-9] = j - w;
    a[i][j-8] = v - w;
    a[i][j-7] = i - w;
    a[i][j-6] = v - w;
    a[i][j-5] = i - w;
    a[i][j-4] = v - w;
    a[i][j-3] = i - w;
    a[i][j-2] = v - w;
    a[i][j-1] = i - w;
    a[i][36] = v - w;
    a[i][37] = v - w;
    a[i][38] = v - w;
    a[i][35] = i - w;
    a[i][34] = i - w;
    a[i-4][j-25] = v - w;
    a[i][j-26] = v - w;
    a[i][j-27] = v - w;
    a[i-1][j-26] = j - w;
    a[i][j-27] = v - w;
    a[i][j-28] = v - w;
    a[i][j-29] = j - w;
    a[i][j-30] = v - w;
    a[i][j-31] = v - w;
    a[i][j-32] = v - w;
    a[i][j-33] = v - w;
    a[i][j-11] = v - w;
    a[i][j-10] = v - w;
    a[i][j-9] = j - w;
    a[i][j-8] = v - w;
    a[i][j-7] = i - w;
    a[i][j-6] = v - w;
    a[i][j-5] = i - w;
    a[i][j-4] = v - w;
    a[i][j-3] = i - w;
    a[i][j-2] = v - w;
    a[i][j-1] = i - w;
    a[i][36] = v - w;
    a[i][37] = v - w;
    a[i][38] = v - w;
    a[i][35] = i - w;
    a[i][34] = i - w;
    i = i + 3;
    a[i-1][0] = v * w;
    a[i][1] = j + w;
    a[i][3] = v * w;
    a[i-1][4] = v * w;
    a[i][5] = j + w;
    a[i][6] = v * w;
    a[i][7] = v * w;
    a[i][8] = v * w;
    a[i][9] = v * w;
    a[i][10] = v * w;
    a[i][j-9] = j + w;
    a[i][j-8] = v * w;
    a[i][j-7] = j + w;
    a[i][j-6] = v * w;
    a[i-2][j-5] = j + w;
    a[i][j-4] = v * w;
    a[i][j-3] = j + w;
    a[i][j-2] = v * w;
    a[i][j-1] = j + w;
    a[i][11] = v * w;
    a[i][12] = v * w;
    a[i][13] = j + w;
    a[i][14] = v * w;
    a[i][15] = v * w;
    a[i][16] = v * w;
    a[i][17] = v * w;
    a[i-3][18] = j + w;
    a[i][19] = v * w;
    a[i][20] = v + w;
    a[i][21] = v + w;
    i = i - 3;
    a[i][22] = v + w;
    a[i][23] = j + w;
    a[i][24] = v + w;
    a[i][25] = v + w;
    a[i][26] = v + w;
    a[i][27] = v + w;
    a[i][26] = j + w;
    a[i][27] = v + w;
    a[i][28] = v + w;
    a[i][29] = j - w;
    a[i][30] = v + w;
    a[i][31] = v + w;
    a[i][32] = v + w;
    a[i][33] = v + w;
    a[i][j-11] = v + w;
    a[i][j-10] = v + w;
    a[i][j-9] = j - w;
    a[i][j-8] = v + w;
    a[i][j-7] = j - w;
    a[i][j-6] = v + w;
    a[i][j-5] = j - w;
    a[i][j-4] = v + w;
    a[i][j-3] = j - w;
    a[i][j-2] = v + w;
    a[i][j-1] = j - w;
    a[i][j-3] = v + w;
    a[i][j-4] = v + w;
    a[i][j-5] = v + w;
    a[i][j-35] = j - w;
    a[i][j-34] = j - w;
    a[i][j-15] = v + w;
    a[i][j-16] = v + w;
    a[i][j-17] = v + w;
    a[i][j-18] = j - w;
    a[i][j-19] = v + w;
    a[i][j-20] = v + w;
    a[i][j-21] = v - w;
    a[i][j-22] = v - w;
    a[i][j-23] = j - w;
    a[i][j-24] = v - w;
    i = i + 4;
    a[i][j-25] = v - w;
    a[i][j-26] = v - w;
    a[i][j-27] = v - w;
    a[i][j-26] = j - w;
    a[i][j-27] = v - w;
    a[i][j-28] = v - w;
    a[i][j-29] = j - w;
    a[i][j-30] = v - w;
    a[i][j-31] = v - w;
    a[i][j-32] = v - w;
    a[i][j-33] = v - w;
    a[i][j-11] = v - w;
    a[i][j-10] = v - w;
    a[i][j-9] = j - w;
    a[i][j-8] = v - w;
    a[i][j-7] = i - w;
    a[i][j-6] = v - w;
    a[i][j-5] = i - w;
    a[i][j-4] = v - w;
    a[i][j-3] = i - w;
    a[i][j-2] = v - w;
    a[i][j-1] = i - w;
    a[i][36] = v - w;
    a[i][37] = v - w;
    a[i][38] = v - w;
    a[i][35] = i - w;
    a[i][34] = i - w;
    i = i - 1;
    a[i][j-25] = v - w;
    a[i][j-26] = v - w;
    a[i][j-27] = v - w;
    a[i][j-26] = j - w;
    a[i][j-27] = v - w;
    a[i][j-28] = v - w;
    a[i][j-29] = j - w;
    a[i][j-30] = v - w;
    a[i][j-31] = v - w;
    a[i][j-32] = v - w;
    a[i][j-33] = v - w;
    a[i][j-11] = v - w;
    a[i][j-10] = v - w;
    a[i][j-9] = j - w;
    a[i][j-8] = v - w;
    a[i][j-7] = i - w;
    a[i][j-6] = v - w;
    a[i][j-5] = i - w;
    a[i][j-4] = v - w;
    a[i][j-3] = i - w;
    a[i][j-2] = v - w;
    a[i][j-1] = i - w;
    a[i][36] = v - w;
    a[i][37] = v - w;
    a[i][38] = v - w;
    a[i][35] = i - w;
    a[i][34] = i - w;
    i = i - 3;
    a[i][0] = v * w;
    a[i][1] = j + w;
    a[i][3] = v * w;
    a[i-1][4] = v * w;
    a[i][5] = j + w;
    a[i][6] = v * w;
    a[i][7] = v * w;
    a[i][8] = v * w;
    a[i][9] = v * w;
    a[i][10] = v * w;
    a[i][j-9] = j + w;
    a[i][j-8] = v * w;
    a[i][j-7] = j + w;
    a[i][j-6] = v * w;
    a[i-2][j-5] = j + w;
    a[i][j-4] = v * w;
    a[i][j-3] = j + w;
    a[i][j-2] = v * w;
    a[i][j-1] = j + w;
    a[i][11] = v * w;
    a[i][12] = v * w;
    a[i][13] = j + w;
    a[i][14] = v * w;
    a[i][15] = v * w;
    a[i][16] = v * w;
    a[i][17] = v * w;
    a[i-3][18] = j + w;
    a[i][19] = v * w;
    a[i][20] = v + w;
    a[i][21] = v + w;
    i = i + 3;
    a[i][22] = v + w;
    a[i][23] = j + w;
    a[i][24] = v + w;
    a[i][25] = v + w;
    a[i][26] = v + w;
    a[i][27] = v + w;
    a[i][26] = j + w;
    a[i][27] = v + w;
    a[i][28] = v + w;
    a[i][29] = j - w;
    a[i][30] = v + w;
    a[i][31] = v + w;
    a[i][32] = v + w;
    a[i][33] = v + w;
    a[i][j-11] = v + w;
    a[i][j-10] = v + w;
    a[i][j-9] = j - w;
    a[i][j-8] = v + w;
    a[i][j-7] = j - w;
    a[i][j-6] = v + w;
    a[i][j-5] = j - w;
    a[i][j-4] = v + w;
    a[i][j-3] = j - w;
    a[i][j-2] = v + w;
    a[i][j-1] = j - w;
    a[i][j-3] = v + w;
    a[i][j-4] = v + w;
    a[i][j-5] = v + w;
    a[i][j-35] = j - w;
    a[i][j-34] = j - w;
    a[i][j-15] = v + w;
    a[i][j-16] = v + w;
    a[i][j-17] = v + w;
    a[i][j-18] = j - w;
    a[i][j-19] = v + w;
    a[i][j-20] = v + w;
    a[i][j-21] = v - w;
    a[i][j-22] = v - w;
    a[i][j-23] = j - w;
    a[i][j-24] = v - w;
    i = i + 2;
    a[i][j-25] = v - w;
    a[i][j-26] = v - w;
    a[i][j-27] = v - w;
    a[i][j-26] = j - w;
    a[i][j-27] = v - w;
    a[i][j-28] = v - w;
    a[i][j-29] = j - w;
    a[i][j-30] = v - w;
    a[i][j-31] = v - w;
    a[i][j-32] = v - w;
    a[i][j-33] = v - w;
    a[i][j-11] = v - w;
    a[i][j-10] = v - w;
    a[i][j-9] = j - w;
    a[i][j-8] = v - w;
    a[i][j-7] = i - w;
    a[i][j-6] = v - w;
    a[i][j-5] = i - w;
    a[i][j-4] = v - w;
    a[i][j-3] = i - w;
    a[i][j-2] = v - w;
    a[i][j-1] = i - w;
    a[i][36] = v - w;
    a[i][37] = v - w;
    a[i][38] = v - w;
    a[i][35] = i - w;
    a[i][34] = i - w;
    a[i-4][j-25] = v - w;
    a[i][j-26] = v - w;
    a[i][j-27] = v - w;
    a[i-1][j-26] = j - w;
    a[i][j-27] = v - w;
    a[i][j-28] = v - w;
    a[i][j-29] = j - w;
    a[i][j-30] = v - w;
    a[i][j-31] = v - w;
    a[i][j-32] = v - w;
    a[i][j-33] = v - w;
    a[i][j-11] = v - w;
    a[i][j-10] = v - w;
    a[i][j-9] = j - w;
    a[i][j-8] = v - w;
    a[i][j-7] = i - w;
    a[i][j-6] = v - w;
    a[i][j-5] = i - w;
    a[i][j-4] = v - w;
    a[i][j-3] = i - w;
    a[i][j-2] = v - w;
    a[i][j-1] = i - w;
    a[i][36] = v - w;
    a[i][37] = v - w;
    a[i][38] = v - w;
    a[i][35] = i - w;
    a[i][34] = i - w;
    i = i - 5;
    a[i][0] = v * w;
    a[i][1] = j + w;
    a[i][3] = v * w;
    a[i-1][4] = v * w;
    a[i][5] = j + w;
    a[i][6] = v * w;
    a[i][7] = v * w;
    a[i][8] = v * w;
    a[i][9] = v * w;
    a[i][10] = v * w;
    a[i][j-9] = j + w;
    a[i][j-8] = v * w;
    a[i][j-7] = j + w;
    a[i][j-6] = v * w;
    a[i-2][j-5] = j + w;
    a[i][j-4] = v * w;
    a[i][j-3] = j + w;
    a[i][j-2] = v * w;
    a[i][j-1] = j + w;
    a[i][11] = v * w;
    a[i][12] = v * w;
    a[i][13] = j + w;
    a[i][14] = v * w;
    a[i][15] = v * w;
    a[i][16] = v * w;
    a[i][17] = v * w;
    a[i-3][18] = j + w;
    a[i][19] = v * w;
    a[i][20] = v + w;
    a[i][21] = v + w;
    a[i][22] = v + w;
    a[i][23] = j + w;
    a[i][24] = v + w;
    a[i][25] = v + w;
    a[i][26] = v + w;
    a[i][27] = v + w;
    a[i][26] = j + w;
    i = i - 7;
    a[i][27] = v + w;
    a[i][28] = v + w;
    a[i][29] = j - w;
    a[i][30] = v + w;
    a[i][31] = v + w;
    a[i][32] = v + w;
    a[i][33] = v + w;
    a[i][j-11] = v + w;
    a[i][j-10] = v + w;
    a[i][j-9] = j - w;
    a[i][j-8] = v + w;
    a[i][j-7] = j - w;
    a[i][j-6] = v + w;
    a[i][j-5] = j - w;
    a[i][j-4] = v + w;
    a[i][j-3] = j - w;
    a[i][j-2] = v + w;
    a[i][j-1] = j - w;
    a[i][j-3] = v + w;
    a[i][j-4] = v + w;
    a[i][j-5] = v + w;
    a[i][j-35] = j - w;
    a[i][j-34] = j - w;
    a[i][j-15] = v + w;
    a[i][j-16] = v + w;
    a[i][j-17] = v + w;
    a[i][j-18] = j - w;
    a[i][j-19] = v + w;
    a[i][j-20] = v + w;
    a[i][j-21] = v - w;
    a[i][j-22] = v - w;
    a[i][j-23] = j - w;
    a[i][j-24] = v - w;
    a[i][j-25] = v - w;
    a[i][j-26] = v - w;
    a[i][j-27] = v - w;
    a[i][j-26] = j - w;
    a[i][j-27] = v - w;
    a[i][j-28] = v - w;
    a[i][j-29] = j - w;
    i = i + 3;
    a[i][j-30] = v - w;
    a[i][j-31] = v - w;
    a[i][j-32] = v - w;
    a[i][j-33] = v - w;
    a[i][j-11] = v - w;
    a[i][j-10] = v - w;
    i = i + 4;
    a[i][j-9] = j - w;
    a[i][j-8] = v - w;
    a[i][j-7] = i - w;
    a[i][j-6] = v - w;
    a[i][j-5] = i - w;
    a[i][j-4] = v - w;
    a[i][j-3] = i - w;
    a[i][j-2] = v - w;
    a[i][j-1] = i - w;
    a[i][36] = v - w;
    a[i][37] = v - w;
    a[i][38] = v - w;
    a[i][35] = i - w;
    a[i][34] = i - w;
    a[i][j-25] = v - w;
    a[i][j-26] = v - w;
    a[i][j-27] = v - w;
    a[i][j-26] = j - w;
    a[i][j-27] = v - w;
    a[i][j-28] = v - w;
    a[i][j-29] = j - w;
    a[i][j-30] = v - w;
    a[i][j-31] = v - w;
    a[i][j-32] = v - w;
    a[i][j-33] = v - w;
    a[i][j-11] = v - w;
    a[i][j-10] = v - w;
    a[i][j-9] = j - w;
    a[i][j-8] = v - w;
    a[i][j-7] = i - w;
    a[i][j-6] = v - w;
    a[i][j-5] = i - w;
    a[i][j-4] = v - w;
    a[i][j-3] = i - w;
    a[i][j-2] = v - w;
    a[i][j-1] = i - w;
    a[i][36] = v - w;
    a[i][37] = v - w;
    a[i][38] = v - w;
    a[i][35] = i - w;
    a[i][34] = i - w;
    a[i][0] = v * w;
    a[i][1] = j + w;
    a[i][3] = v * w;
    a[i-1][4] = v * w;
    a[i][5] = j + w;
    a[i][6] = v * w;
    a[i][7] = v * w;
    a[i][8] = v * w;
    i = i + 2;
    a[i][9] = v * w;
    a[i][10] = v * w;
    a[i][j-9] = j + w;
    a[i][j-8] = v * w;
    a[i][j-7] = j + w;
    a[i][j-6] = v * w;
    a[i-2][j-5] = j + w;
    a[i][j-4] = v * w;
    a[i][j-3] = j + w;
    a[i][j-2] = v * w;
    a[i][j-1] = j + w;
    a[i][11] = v * w;
    a[i][12] = v * w;
    a[i][13] = j + w;
    a[i][14] = v * w;
    a[i][15] = v * w;
    a[i][16] = v * w;
    a[i][17] = v * w;
    a[i-3][18] = j + w;
    a[i][19] = v * w;
    a[i][20] = v + w;
    a[i][21] = v + w;
    a[i][22] = v + w;
    a[i][23] = j + w;
    a[i][24] = v + w;
    a[i][25] = v + w;
    a[i][26] = v + w;
    a[i][27] = v + w;
    a[i][26] = j + w;
    a[i][27] = v + w;
    a[i][28] = v + w;
    a[i][29] = j - w;
    a[i][30] = v + w;
    a[i][31] = v + w;
    a[i][32] = v + w;
    a[i][33] = v + w;
    a[i][j-11] = v + w;
    a[i][j-10] = v + w;
    a[i][j-9] = j - w;
    a[i][j-8] = v + w;
    a[i][j-7] = j - w;
    i = i + 1;
    a[i][j-6] = v + w;
    a[i][j-5] = j - w;
    a[i][j-4] = v + w;
    a[i][j-3] = j - w;
    a[i][j-2] = v + w;
    a[i][j-1] = j - w;
    a[i][j-3] = v + w;
    a[i][j-4] = v + w;
    a[i][j-5] = v + w;
    a[i][j-35] = j - w;
    a[i][j-34] = j - w;
    a[i][j-15] = v + w;
    a[i][j-16] = v + w;
    a[i][j-17] = v + w;
    a[i][j-18] = j - w;
    a[i][j-19] = v + w;
    a[i][j-20] = v + w;
    a[i][j-21] = v - w;
    a[i][j-22] = v - w;
    a[i][j-23] = j - w;
    a[i][j-24] = v - w;
    a[i][j-25] = v - w;
    a[i][j-26] = v - w;
    a[i][j-27] = v - w;
    a[i][j-26] = j - w;
    a[i][j-27] = v - w;
    a[i][j-28] = v - w;
    i = i - 3;
    a[i][j-29] = j - w;
    a[i][j-30] = v - w;
    a[i][j-31] = v - w;
    a[i][j-32] = v - w;
    a[i][j-33] = v - w;
    a[i][j-11] = v - w;
    a[i][j-10] = v - w;
    a[i][j-9] = j - w;
    a[i][j-8] = v - w;
    a[i][j-7] = i - w;
    a[i][j-6] = v - w;
    a[i][j-5] = i - w;
    a[i][j-4] = v - w;
    a[i][j-3] = i - w;
    a[i][j-2] = v - w;
    a[i][j-1] = i - w;
    a[i][36] = v - w;
    a[i][37] = v - w;
    a[i][38] = v - w;
    a[i][35] = i - w;
    a[i][34] = i - w;
    a[i-4][j-25] = v - w;
    a[i][j-26] = v - w;
    a[i][j-27] = v - w;
    a[i-1][j-26] = j - w;
    a[i][j-27] = v - w;
    a[i][j-28] = v - w;
    a[i][j-29] = j - w;
    a[i][j-30] = v - w;
    a[i][j-31] = v - w;
    a[i][j-32] = v - w;
    a[i][j-33] = v - w;
    a[i][j-11] = v - w;
    a[i][j-10] = v - w;
    a[i][j-9] = j - w;
    a[i][j-8] = v - w;
    a[i][j-7] = i - w;
    a[i][j-6] = v - w;
    a[i][j-5] = i - w;
    i = i - 4;
    a[i][j-4] = v - w;
    a[i][j-3] = i - w;
    a[i][j-2] = v - w;
    a[i][j-1] = i - w;
    a[i][36] = v - w;
    a[i][37] = v - w;
    a[i][38] = v - w;
    a[i][35] = i - w;
    a[i][34] = i - w;
    a[i][0] = v * w;
    a[i][1] = j + w;
    a[i][3] = v * w;
    a[i-1][4] = v * w;
    a[i][5] = j + w;
    a[i][6] = v * w;
    a[i][7] = v * w;
    a[i][8] = v * w;
    a[i][9] = v * w;
    a[i][10] = v * w;
    a[i][j-9] = j + w;
    a[i][j-8] = v * w;
    a[i][j-7] = j + w;
    a[i][j-6] = v * w;
    a[i-2][j-5] = j + w;
    a[i][j-4] = v * w;
    a[i][j-3] = j + w;
    a[i][j-2] = v * w;
    a[i][j-1] = j + w;
    a[i][11] = v * w;
    a[i][12] = v * w;
    a[i][13] = j + w;
    a[i][14] = v * w;
    a[i][15] = v * w;
    a[i][16] = v * w;
    a[i][17] = v * w;
    i = i + 2;
    a[i-3][18] = j + w;
    a[i][19] = v * w;
    a[i][20] = v + w;
    a[i][21] = v + w;
    a[i][22] = v + w;
    a[i][23] = j + w;
    a[i][24] = v + w;
    a[i][25] = v + w;
    a[i][26] = v + w;
    a[i][27] = v + w;
    a[i][26] = j + w;
    a[i][27] = v + w;
    a[i][28] = v + w;
    a[i][29] = j - w;
    a[i][30] = v + w;
    a[i][31] = v + w;
    a[i][32] = v + w;
    a[i][33] = v + w;
    a[i][j-11] = v + w;
    a[i][j-10] = v + w;
    a[i][j-9] = j - w;
    a[i][j-8] = v + w;
    a[i][j-7] = j - w;
    a[i][j-6] = v + w;
    a[i][j-5] = j - w;
    a[i][j-4] = v + w;
    i = i + 3;
    a[i][j-3] = j - w;
    a[i][j-2] = v + w;
    a[i][j-1] = j - w;
    a[i][j-3] = v + w;
    a[i][j-4] = v + w;
    a[i][j-5] = v + w;
    a[i][j-35] = j - w;
    a[i][j-34] = j - w;
    a[i][j-15] = v + w;
    a[i][j-16] = v + w;
    a[i][j-17] = v + w;
    a[i][j-18] = j - w;
    a[i][j-19] = v + w;
    a[i][j-20] = v + w;
    a[i][j-21] = v - w;
    a[i][j-22] = v - w;
    a[i][j-23] = j - w;
    a[i][j-24] = v - w;
    a[i][j-25] = v - w;
    a[i][j-26] = v - w;
    a[i][j-27] = v - w;
    a[i][j-26] = j - w;
    a[i][j-27] = v - w;
    a[i][j-28] = v - w;
    a[i][j-29] = j - w;
    a[i][j-30] = v - w;
    a[i][j-31] = v - w;
    a[i][j-32] = v - w;
    a[i][j-33] = v - w;
    a[i][j-11] = v - w;
    a[i][j-10] = v - w;
    a[i][j-9] = j - w;
    a[i][j-8] = v - w;
    a[i][j-7] = i - w;
    a[i][j-6] = v - w;
    a[i][j-5] = i - w;
    a[i][j-4] = v - w;
    a[i][j-3] = i - w;
    a[i][j-2] = v - w;
    a[i][j-1] = i - w;
    a[i][36] = v - w;
    a[i][37] = v - w;
    a[i][38] = v - w;
    a[i][35] = i - w;
    a[i][34] = i - w;
    i = i - 1;
    a[i][j-25] = v - w;
    a[i][j-26] = v - w;
    a[i][j-27] = v - w;
    a[i][j-26] = j - w;
    a[i][j-27] = v - w;
    a[i][j-28] = v - w;
    a[i][j-29] = j - w;
    a[i][j-30] = v - w;
    a[i][j-31] = v - w;
    a[i][j-32] = v - w;
    a[i][j-33] = v - w;
    a[i][j-11] = v - w;
    a[i][j-10] = v - w;
    a[i][j-9] = j - w;
    a[i][j-8] = v - w;
    a[i][j-7] = i - w;
    a[i][j-6] = v - w;
    a[i][j-5] = i - w;
    a[i][j-4] = v - w;
    a[i][j-3] = i - w;
    a[i][j-2] = v - w;
    a[i][j-1] = i - w;
    a[i][36] = v - w;
    a[i][37] = v - w;
    a[i][38] = v - w;
    a[i][35] = i - w;
    a[i][34] = i - w;
    a[i][0] = v * w;
    a[i][1] = j + w;
    a[i][3] = v * w;
    a[i-1][4] = v * w;
    a[i][5] = j + w;
    a[i][6] = v * w;
    a[i][7] = v * w;
    a[i][8] = v * w;
    a[i][9] = v * w;
    a[i][10] = v * w;
    a[i][j-9] = j + w;
    a[i][j-8] = v * w;
    a[i][j-7] = j + w;
    a[i][j-6] = v * w;
    a[i-2][j-5] = j + w;
    a[i][j-4] = v * w;
    a[i][j-3] = j + w;
    a[i][j-2] = v * w;
    a[i][j-1] = j + w;
    a[i][11] = v * w;
    a[i][12] = v * w;
    a[i][13] = j + w;
    a[i][14] = v * w;
    a[i][15] = v * w;
    a[i][16] = v * w;
    a[i][17] = v * w;
    a[i-3][18] = j + w;
    a[i][19] = v * w;
    a[i][20] = v + w;
    a[i][21] = v + w;
    i = i + 3;
    a[i][22] = v + w;
    a[i][23] = j + w;
    a[i][24] = v + w;
    a[i][25] = v + w;
    a[i][26] = v + w;
    a[i][27] = v + w;
    a[i][26] = j + w;
    a[i][27] = v + w;
    a[i][28] = v + w;
    a[i][29] = j - w;
    a[i][30] = v + w;
    a[i][31] = v + w;
    a[i][32] = v + w;
    a[i][33] = v + w;
    a[i][j-11] = v + w;
    a[i][j-10] = v + w;
    a[i][j-9] = j - w;
    a[i][j-8] = v + w;
    a[i][j-7] = j - w;
    a[i][j-6] = v + w;
    a[i][j-5] = j - w;
    a[i][j-4] = v + w;
    a[i][j-3] = j - w;
    a[i][j-2] = v + w;
    a[i][j-1] = j - w;
    a[i][j-3] = v + w;
    a[i][j-4] = v + w;
    a[i][j-5] = v + w;
    a[i][j-35] = j - w;
    a[i][j-34] = j - w;
    a[i][j-15] = v + w;
    a[i][j-16] = v + w;
    a[i][j-17] = v + w;
    a[i][j-18] = j - w;
    i = i + 2;
    a[i][j-19] = v + w;
    a[i][j-20] = v + w;
    a[i][j-21] = v - w;
    a[i][j-22] = v - w;
    a[i][j-23] = j - w;
    a[i][j-24] = v - w;
    a[i][j-25] = v - w;
    a[i][j-26] = v - w;
    a[i][j-27] = v - w;
    a[i][j-26] = j - w;
    a[i][j-27] = v - w;
    a[i][j-28] = v - w;
    a[i][j-29] = j - w;
    a[i][j-30] = v - w;
    a[i][j-31] = v - w;
    a[i][j-32] = v - w;
    a[i][j-33] = v - w;
    a[i][j-11] = v - w;
    a[i][j-10] = v - w;
    a[i][j-9] = j - w;
    a[i][j-8] = v - w;
    a[i][j-7] = i - w;
    a[i][j-6] = v - w;
    a[i][j-5] = i - w;
    a[i][j-4] = v - w;
    a[i][j-3] = i - w;
    a[i][j-2] = v - w;
    a[i][j-1] = i - w;
    a[i][36] = v - w;
    a[i][37] = v - w;
    a[i][38] = v - w;
    a[i][35] = i - w;
    a[i][34] = i - w;
    a[i-4][j-25] = v - w;
    a[i][j-26] = v - w;
    a[i][j-27] = v - w;
    a[i-1][j-26] = j - w;
    a[i][j-27] = v - w;
    a[i][j-28] = v - w;
    a[i][j-29] = j - w;
    a[i][j-30] = v - w;
    a[i][j-31] = v - w;
    a[i][j-32] = v - w;
    a[i][j-33] = v - w;
    a[i][j-11] = v - w;
    a[i][j-10] = v - w;
    i = i - 5;
    a[i][j-9] = j - w;
    a[i][j-8] = v - w;
    a[i][j-7] = i - w;
    a[i][j-6] = v - w;
    a[i][j-5] = i - w;
    a[i][j-4] = v - w;
    a[i][j-3] = i - w;
    a[i][j-2] = v - w;
    a[i][j-1] = i - w;
    a[i][36] = v - w;
    a[i][37] = v - w;
    a[i][38] = v - w;
    a[i][35] = i - w;
    a[i][34] = i - w;
    i = i - 1;
    a[i][j-6] = v - w;
    a[i][j-5] = i - w;
    a[i][j-4] = v - w;
    a[i][j-3] = i - w;
    a[i][j-2] = v - w;
    a[i][j-1] = i - w;
    a[i][36] = v - w;
    a[i][37] = v - w;
    a[i][38] = v - w;
    a[i][35] = i - w;
    a[i][34] = i - w;
    a[i-4][j-25] = v - w;
    a[i][j-26] = v - w;
    a[i][j-27] = v - w;
    a[i-1][j-26] = j - w;
    a[i][j-27] = v - w;
    a[i][j-28] = v - w;
    a[i][j-29] = j - w;
    a[i][j-30] = v - w;
    a[i][j-31] = v - w;
    a[i][j-32] = v - w;
    a[i][j-33] = v - w;
    a[i][j-11] = v - w;
    a[i][j-10] = v - w;
    a[i][j-9] = j - w;
    a[i][j-8] = v - w;
    a[i][j-7] = i - w;
    a[i][j-6] = v - w;
    a[i][j-5] = i - w;
    a[i][j-4] = v - w;
    a[i][j-3] = i - w;
    a[i][j-2] = v - w;
    a[i][j-1] = i - w;
    a[i][36] = v - w;
    a[i][37] = v - w;
    a[i][38] = v - w;
    i = i + 1;
    a[i][35] = i - w;
    a[i][34] = i - w;
    a[i-1][j-21] = v - w;
    a[i-2][j-22] = v - w;
    a[i-3][j-23] = j - w;
    a[i][j-24] = v - w;
    a[i][j-25] = v - w;
    a[i][j-26] = v - w;
    a[i][j-27] = v - w;
    a[i][j-26] = j - w;
    a[i][j-27] = v - w;
    a[i][j-28] = v - w;
    a[i][j-29] = j - w;
    a[i][j-30] = v - w;
    a[i][j-31] = v - w;
    a[i][j-32] = v - w;
    a[i-4][j-33] = v - w;
    a[i][j-11] = v - w;
    a[i][j-10] = v - w;
    a[i][j-9] = j - w;
    a[i][j-8] = v - w;
    a[i][j-7] = i - w;
    a[i][j-6] = v - w;
    a[i-5][j-5] = i - w;
    a[i+3][j-4] = v - w;
    a[i][j-3] = i - w;
    a[i][j-2] = v - w;
    a[i][j-1] = i - w;
    a[i][36] = v - w;
    a[i+2][37] = v - w;
    a[i][38] = v - w;
    a[i][35] = i - w;
    a[i][34] = i - w;
    a[i-4][j-25] = v - w;
    a[i][j-26] = v - w;
    a[i][j-27] = v - w;
    a[i-1][j-26] = j - w;
    a[i][j-27] = v - w;
    a[i][j-28] = v - w;
    a[i-3][j-29] = j - w;
  }

  for (i = 0; i < N; i++) {
    for (j = 0; j < N; j++) {
      if (a[i][j] < 10) {
	sum += 1;
      }
      else {
        sum += -1;
      }
    }
  }

  printf("sum  = %d --- ", sum); 

  if (sum == ANS) {
    printf("OK\n");
  }
  else {
    printf("NG, Answer => %d\n", ANS);
  }
}
