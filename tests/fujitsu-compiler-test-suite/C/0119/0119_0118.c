#include <stdlib.h>
#include <math.h>
#include  <stdio.h>
int main()
{
struct {
       struct { int a[10],b[10] ; } c[10] ;
       int d ;
       struct { int e[10],f[10] ; } g[10] ;
       struct {
              struct { int h,i ; } j[5] ;
              int k ;
              } l[2] ;
       } x[2]
     = {{ {{ {1,1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1,1} },
              { {1,1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1,1} },
              { {1,1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1,1} },
              { {1,1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1,1} },
              { {1,1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1,1} },
              { {1,1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1,1} },
              { {1,1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1,1} },
              { {1,1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1,1} },
              { {1,1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1,1} },
              { {1,1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1,1} }},
            2,
            {{ {3,3,3,3,3,3,3,3,3,3},{3,3,3,3,3,3,3,3,3,3} },
              { {3,3,3,3,3,3,3,3,3,3},{3,3,3,3,3,3,3,3,3,3} },
              { {3,3,3,3,3,3,3,3,3,3},{3,3,3,3,3,3,3,3,3,3} },
              { {3,3,3,3,3,3,3,3,3,3},{3,3,3,3,3,3,3,3,3,3} },
              { {3,3,3,3,3,3,3,3,3,3},{3,3,3,3,3,3,3,3,3,3} },
              { {3,3,3,3,3,3,3,3,3,3},{3,3,3,3,3,3,3,3,3,3} },
              { {3,3,3,3,3,3,3,3,3,3},{3,3,3,3,3,3,3,3,3,3} },
              { {3,3,3,3,3,3,3,3,3,3},{3,3,3,3,3,3,3,3,3,3} },
              { {3,3,3,3,3,3,3,3,3,3},{3,3,3,3,3,3,3,3,3,3} },
              { {3,3,3,3,3,3,3,3,3,3},{3,3,3,3,3,3,3,3,3,3} }},
            {{ {{4,4},{4,4},{4,4},{4,4},{4,4}},4},
              { {{4,4},{4,4},{4,4},{4,4},{4,4}},4} }
         },
         { {{ {1,1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1,1} },
              { {1,1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1,1} },
              { {1,1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1,1} },
              { {1,1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1,1} },
              { {1,1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1,1} },
              { {1,1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1,1} },
              { {1,1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1,1} },
              { {1,1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1,1} },
              { {1,1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1,1} },
              { {1,1,1,1,1,1,1,1,1,1},{1,1,1,1,1,1,1,1,1,1} }},
            2,
            {{ {3,3,3,3,3,3,3,3,3,3},{3,3,3,3,3,3,3,3,3,3} },
              { {3,3,3,3,3,3,3,3,3,3},{3,3,3,3,3,3,3,3,3,3} },
              { {3,3,3,3,3,3,3,3,3,3},{3,3,3,3,3,3,3,3,3,3} },
              { {3,3,3,3,3,3,3,3,3,3},{3,3,3,3,3,3,3,3,3,3} },
              { {3,3,3,3,3,3,3,3,3,3},{3,3,3,3,3,3,3,3,3,3} },
              { {3,3,3,3,3,3,3,3,3,3},{3,3,3,3,3,3,3,3,3,3} },
              { {3,3,3,3,3,3,3,3,3,3},{3,3,3,3,3,3,3,3,3,3} },
              { {3,3,3,3,3,3,3,3,3,3},{3,3,3,3,3,3,3,3,3,3} },
              { {3,3,3,3,3,3,3,3,3,3},{3,3,3,3,3,3,3,3,3,3} },
              { {3,3,3,3,3,3,3,3,3,3},{3,3,3,3,3,3,3,3,3,3} }},
            {{ {{4,4},{4,4},{4,4},{4,4},{4,4}},4},
              { {{4,4},{4,4},{4,4},{4,4},{4,4}},4}}
         } } ;

  printf("***** SCPII38 ***** \n") ;

  if (x[1].d == 2 && x[0].c[5].a[2] == 1 &&
      x[1].l[1].k == 4 && x[0].g[7].f[8] == 3)
    printf("  **** O  K **** \n") ;
  else
    printf("  **** N  G **** \n") ;

  printf("***** E N D   ***** \n") ;
exit (0);
}
