
#include "stdio.h"
#include "math.h"
int main()
{
  int i,j,k,m;
  float  a4[10][10],b4[10][10],c4[10][10];
  double a8[10][10],b8[10][10],c8[10][10];
  float  result4;
  double result8;

  for (j=0;j<10;j++){
    for (i=0;i<10;i++){
      a4[j][i] = i+2;
      b4[j][i] = j+1;
      c4[j][i] = j+1;
      a8[j][i] = i+2;
      b8[j][i] = j+1;
      c8[j][i] = j+1;
    }
  }

  
#pragma omp parallel for
  for (j=0;j<10;j++){
    for (k=0;k<10;k++){
      for (i=0;i<10;i++){
        c4[j][2]=c4[j][2]+a4[k][i]*b4[2][k];
      }
    }
  }
  result4 =0;
  for (j=0;j<10;j++){
    for (i=0;i<10;i++){
      result4=result4+c4[j][i];
    }
  }
  printf("test-1 = %f \n",result4);

  
  m=1;
#pragma omp parallel for
  for (j=0;j<10;j++){
    for (k=0;k<10;k++){
      for (i=0;i<10;i++){
        c8[j][i]=c8[j][i]+a8[k][3]*b8[m][k];
      }
    }
  }
  result8 =0;
  for (j=0;j<10;j++){
    for (i=0;i<10;i++){
      result8=result8+c8[j][i];
    }
  }
  printf("test-2 = %f \n",result8);
}
