#include <stdio.h>
#include <stdlib.h>
#include <math.h>

#ifdef _OPENMP
#include <omp.h>
#endif

int main()
{
      int  ia[50], ib[50], ier;
      int  loop=50;
      int  i;

      for(i=0;i<50;i++)
	{
          ia[i] = 0;
          ib[i] = 0;
	}

      for (i=0; i<loop; i++)
	{
          if((i%3) != 0) ia[i]=(i+1);
        }
      for(i=0;i<50;i++)
	{
          if( ia[i]==0 )
	    {
#pragma omp parallel
{
         ib[i] = 9;
}
	    }
        }

      ier=0;
      for (i=0; i<loop; i++)
	{
         if(ia[i]==(i+1))
	   {
             if(ib[i]!=0) ier=ier+1;
           }
	 else if(ia[i]==0)
	   {
             if(ib[i]!=9) ier=ier+1;
           }
	 else
	   {
             ier=ier+1;
           }
      }
      printf( "-----  --");
      printf( " parallel (where) -----\n");
      if(ier == 0) {
         printf( "OK\n");
      }else{
         printf( " NG!  PARALLEL Directive is not active!\n");
	 for(i=0;i<50;i++)
	   {
	     if(i==0) printf("      ia=");
             printf( "%d ", ia[i]);
	   }
         printf("\n");
	 for(i=0;i<50;i++)
	   {
	     if(i==0) printf("      ib=");
             printf( "%d ", ib[i]);
	   }
         printf("\n");
         printf( "      ier=%d\n", ier);
      }
   exit (0) ;
}