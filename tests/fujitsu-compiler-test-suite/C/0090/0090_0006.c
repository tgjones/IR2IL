
#include <stdio.h>
#include <math.h>

int main(){
  double A[10][10],B[10][10],M[10][10],N[10][10];
  double C,D;
  int i,j;

  for(i=0;i<10;i++){
    for(j=0;j<10;j++){
      C = i+j;
      A[j][i] = pow(C,C);
      D = j*3;
      B[j][i] = pow(D,D);
    }
  }

  for(i=0;i<10;i++){
    for(j=0;j<10;j++){
      printf("adxd A[%d][%d]=%f\n",j,i,A[j][i]);
    }
  }

  for(i=0;i<10;i++){
    for(j=0;j<10;j++){
      printf("adxd B[%d][%d]=%f\n",j,i,B[j][i]);
    }
  }

  for(i=0;i<10;i++){
    for(j=0;j<10;j++){
      M[j][i] = j * 1;
      A[j][i] = pow(M[j][i],M[j][i]);
      N[j][i] = j + 3 * i;
      B[j][i] = pow(N[j][i],N[j][i]);
    }
  }

  for(i=0;i<10;i++){
    for(j=0;j<10;j++){
      printf("(i)adxd A[%d][%d]=%f\n",j,i,A[j][i]);
    }
  }

  for(i=0;i<10;i++){
    for(j=0;j<10;j++){
      printf("(i)adxd B[%d][%d]=%f\n",j,i,B[j][i]);
    }
  }

  for(i=0;i<10;i++){
    for(j=0;j<10;j++){
      A[j][i] = pow(((double)5.0)+i,((double)8.0)+i);
      B[j][i] = pow(((double)6.0)+j,((double)2.0)+j);
    }
  }

  for(i=0;i<10;i++){
    for(j=0;j<10;j++){
      printf("(cnt)adxd A[%d][%d]=%f\n",j,i,A[j][i]);
    }
  }

  for(i=0;i<10;i++){
    for(j=0;j<10;j++){
      printf("(cnt)adxd B[%d][%d]=%f\n",j,i,B[j][i]);
    }
  }

  return 0;
}
