#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#ifdef _OPENMP
#include <omp.h>
#endif

int main() { 
      int ia, ib, ic, id, ie[50];
      int i, loop=50;
      ia = 9;
      ib = -17;
      for (i=0; i<loop; i+=1){ 
         ie[i] = 7*i/3+pow(-1,(i-(i/2)*2));
      }
#pragma omp parallel
{
#pragma omp for reduction(+:ia) reduction(-:ib)
      for (i=0; i<loop; i+=1){ 
         ia = ia-i*13;
         ib = ib+i*3+ie[i];
      }
}
      ic = 9;
      id = -17;
      for (i=0; i<loop; i+=1){ 
         ic = ic-i*13;
         id = id+i*3+ie[i];
      }
      printf("-----  --");
      printf(" parallel");
      printf(" for reduction(+:ia) reduction(-:ib) -----\n");
      if(ia==ic && ib==id) {
         printf( "OK\n");
      }else{
         printf( "NG!  REDUCTION(-) clause is incorrect\n");
         printf( "     (+)  ia=%d, It must be %d",ia,ic);
         printf( "     (-)  ib=%d, It must be %d\n",i,id);
      }
      exit (0) ;
      }
