#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#ifdef _OPENMP
#include <omp.h>
#endif

int main() { 
      int i,ia, ib, it;
      int  thread=4;
      ia = 3;
#ifdef _OPENMP
    thread = omp_get_max_threads();
#endif
#pragma omp parallel reduction(&:ia) private(it)
{
#ifdef _OPENMP
    it = omp_get_thread_num();
#endif
      ia = it*8+3;
}
      ib = 3;
      for (i=0; i<thread; i+=1){ 
          ib = ib & (i*8+3);
      }
      printf("-----  --");
      printf(" parallel reduction(&:ia) private(it) -----\n");
      if(ia==ib) {
         printf( "OK\n");
      }else{
         printf( "NG!  REDUCTION(AND) clause is incorrect!\n");
         printf( "     ia=%d,   It must be %d\n",ia, ib);
      }
      exit (0) ;
      }
