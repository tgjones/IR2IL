#include <stdio.h>
int foo( int *ia, int i )
{
    int isum=0 ;
    for ( ; i < 10 ; ) {
        i++ ;
        isum = isum + *(ia+(i-1)) ;
        *(ia+i) = isum + 2 ;
    }
    return isum ;
}
int main()
{
    int i,ima[11] ;
    for (i=0; i<10; i++) ima[i]=i ;
    if ( foo(ima,1) == 766 )
	puts("OK") ;
    else
	puts("NG") ;
   return 0 ;
}
