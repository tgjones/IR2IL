
#include "stdio.h"
#include "math.h"
int main()
{
  int i,j,k;
  float a[10][11]={1,2,3,1,2,3,1,2,3,1,2,3,1,2,3,1,2,3,1,2,
                   3,1,2,3,1,2,3,1,2,3,1,2,3,1,2,3,1,2,3,1,
                   2,3,1,2,3,1,2,3,1,2,3,1,2,3,1,2,3,1,2,3,
                   1,2,3,1,2,3,1,2,3,1,2,3,1,2,3,1,2,3,1,2,
                   3,1,2,3,1,2,3,1,2,3,1,2,3,1,2,3,1,2,3,1,
                   3,1,2,3,1,2,3,1,2,3};
  float b[10][11]={3,4,5,3,4,5,3,4,5,3,4,5,3,4,5,3,4,5,3,4,
                   5,3,4,5,3,4,5,3,4,5,3,4,5,3,4,5,3,4,5,3,
                   4,5,3,4,5,3,4,5,3,4,5,3,4,5,3,4,5,3,4,5,
                   3,4,5,3,4,5,3,4,5,3,4,5,3,4,5,3,4,5,3,4,
                   5,3,4,5,3,4,5,3,4,5,3,4,5,3,4,5,3,4,5,3,
                   5,3,4,5,3,4,5,3,4,5};
  float c[10][11];
  float result;

  for (j=0;j<10;j++){
    for (i=0;i<11;i++){
     c[j][i] = 1;
   }
  }

#pragma omp parallel for
  for (j=0;j<10;j++){
    for(k=0;k<10;k++){
      for (i=0;i<10;i++){
	c[j][i]=c[j][i]+a[k][i]*b[j][k];
      }
    }
  }
  result =0;
  for (j=0;j<10;j++){
    for (i=0;i<11;i++){
      result=result+c[j][i];
    }
  }
  printf("result=%f \n",result);
}
