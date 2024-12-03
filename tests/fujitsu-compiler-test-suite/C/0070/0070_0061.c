#include <stdio.h>
#include <stdlib.h>
#include <math.h>

#ifdef _OPENMP
#include <omp.h>
#endif

int main()
{
      int ia[50], ib, ic[50], it, ier;
      int  loop=50;
      int  i;

      for(i=0; i<loop; i++)
	{
          ia[i] = 0;
	}
      ib = 0;

#pragma omp parallel private(it)
{
#ifdef _OPENMP
    it = omp_get_thread_num() + 1;
#endif
#pragma omp for schedule(dynamic) ordered
      for(i=0; i<loop; i++)
	{
#pragma omp ordered
   {
          ib = ib + 1;
          ia[i] = ib;
   }
         ic[i] = it;
        }
}
      ier = 0;
      for(i=0; i<loop; i++)
	{
          if(ia[i]!=(i+1)) ier=ier+1 ;
        }
      printf( "-----  --");
      printf( " parallel for schedule(dynamic) orderd -----\n");
      if(ier==0) {
         printf( "OK\n");
      }else{
         printf( "NG!  DO ORDERD directive is not active\n");
	 for(i=0; i<loop; i++)
	   {
             if(i==0) printf( "     ia=");
	     printf("%d ",ia[i]);
	   }
         printf( "\n");
	 for(i=0; i<loop; i++)
	   {
             if(i==0) printf( "     ic=");
	     printf("%d ",ic[i]);
	   }
         printf( "\n");
      }
   exit (0) ;
}
