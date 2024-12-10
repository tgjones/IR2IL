#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#ifdef _OPENMP
#include <omp.h>
#endif

#define n 47
      void sub1(double zzz[n][n][n],double aaa[n][n][n],int k)
{
      double tmp;
      int i,j;

#pragma omp parallel for private(tmp,j,i)
        for (j=0; j<n; j+=1){ 
          for (i=0; i<n; i+=1){ 
            tmp = zzz[i][j][k]+zzz[i][j][k]+zzz[i][j][k];
            aaa[i][j][k] = sqrt(pow(tmp,2));
          }
        }
      return;
      }

#define n 47
      void sub2(double zzz[n][n][n],double aaa[n][n][n],int k,int j)
{
      double tmp;
      int i;

#pragma omp parallel for private(tmp,i)
          for (i=0; i<n; i+=1){ 
            tmp = zzz[i][j][k]+zzz[i][j][k]+zzz[i][j][k];
            aaa[i][j][k] = sqrt(pow(tmp,2));
          }
      return;
      }

#define n 47
      void sub3(double zzz[n][n][n],double aaa[n][n][n],int k,int j)
{
      double tmp;
      int i;

#pragma omp parallel for private(tmp,i)
          for (i=0; i<n; i+=1){ 
            tmp = zzz[i][j][k]+zzz[i][j][k]+zzz[i][j][k];
            aaa[i][j][k] = sqrt(pow(tmp,2));
          }
      return;
      }

int main() { 
#define n 47
      double zzz[n][n][n],c_zzz[n][n][n];
      double aaa[n][n][n],c_aaa[n][n][n];
      double tmp;
       int i,j,k,i_err;

      for (k=0; k<n; k+=1){ 
        for (j=0; j<n; j+=1){ 
          for (i=0; i<n; i+=1){ 
            zzz[i][j][k]=(float)((k)*100+(j)*10+i);
            aaa[i][j][k]=0.0;
            c_zzz[i][j][k]=(float)((k)*100+(j)*10+i);
            c_aaa[i][j][k]=0.0;
          }
        }
      }

#pragma omp parallel for
      for (k=0; k<n; k+=1){ 
        sub1 (zzz,aaa,k);
      }
#pragma omp parallel for
      for (k=0; k<n; k+=1){ 
        for (j=0; j<n; j+=1){ 
          sub2 (zzz,aaa,k,j);
        }
      }
      for (k=0; k<n; k+=1){ 
#pragma omp parallel for
        for (j=0; j<n; j+=1){ 
          sub3 (zzz,aaa,k,j);
        }
      }

      for (k=0; k<n; k+=1){ 
        for (j=0; j<n; j+=1){ 
          for (i=0; i<n; i+=1){ 
            tmp = c_zzz[i][j][k]+c_zzz[i][j][k]+c_zzz[i][j][k];
            c_aaa[i][j][k] = sqrt(pow(tmp,2));
          }
        }
      }
      i_err=0;
      for (k=0; k<n; k+=1){ 
        for (j=0; j<n; j+=1){ 
          for (i=0; i<n; i+=1){ 
            if (abs((aaa[k][j][i])-(c_aaa[k][j][i])) > 0.000002) i_err=1;
          }
        }
      }
      if (i_err==0) {
        printf(" OK \n");;
      }else{
        printf(" NG \n");;
      }
      exit (0) ;
      }