#include <stdio.h>
int sub(a,b,c,d,n)
  int a,b,c,d,n;
{
  int x,y,i,x1,y1,t1;
  for ( i = 0 ; i < n ; i ++ ) {
    if ( a == 0 ) c = 0 ;
    if ( b == 0 ) d = 0 ;
    x = c + d + i*10 + x1 ;
    y = c + d + i ;
    x1 = c + d ;
    y1 = c * d ;
    if ( a == 0 ) x = 0 ;
    if ( b == 0 ) y = 0 ;
    t1 = x1 + y1 ;
    printf("t1=%d\n",t1);
  }
  return ( x+y ) ;
}
int main()
{
  int i;
  for( i=0 ; i<10 ; i++ ) {
    printf("sub = %d\n",sub(i,10-i,i*10,10-2*i,10) );
  } 
}