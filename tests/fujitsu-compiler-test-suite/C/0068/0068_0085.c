#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#ifdef _OPENMP
#include <omp.h>
#endif

int main() { 
      int ia, ib;
      int i, loop=50;
      ia = 72;
      ib = 72;
#pragma omp parallel
{
#pragma omp for reduction(|:ia)
      for (i=0; i<loop; i+=1){ 
         ia = ia|(i*8);
      }
}
      for (i=0; i<loop; i+=1){ 
         ib = ib|(i*8);
      }
      printf("-----  --");
      printf(" parallel");
      printf(" do reduction(|:ia) -----\n");
      if(ia==ib) {
         printf( "OK\n");
      }else{
         printf( "NG!  REDUCTION(OR) clause is incorrect\n");
         printf( "     ia=%d,  It must be %d\n",ia,ib);
      }
      exit (0) ;
      }
