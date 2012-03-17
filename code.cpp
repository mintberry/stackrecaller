/*ABSTRACT: This 100-line code is an implementation of topological sorting of a directed graph. 
//I coded it in C++ during my sophomore year. I select this piece of code because it's 
//only 100-line long but implements a complete function. In addition, the code is clear 
//to read and the comments are sufficient. For this piece of code, I include the declaration 
//and definition of the functional class. Some data structures you may find in the code,
//like MyQueue, EdgeM, etc. are also implemented by myself. Start from next line, the code 
//is exactly 100-line.*/
class Diagraph{
public:
    Diagraph();
    ~Diagraph();
    void Mixed_Lists();
    void DFSort(List<int> &topological);
    void recursive_DFS(int iVertex, bool *visited, List<int> &topological);
    void BFSort(List<int> &topological);
private:
    int iDots,iEdges;                        //how many vertices and directed edges?
    bool **adjacency;                        //2-dimensional array to keep inputs
    EdgeM **mixed_list;                      //mixed list to store the graph
};
Diagraph::Diagraph(): mixed_list (NULL) {                                                     //constructor, to save inputs to the array
    cout << "Please input the number of vertices and edges,\r\nthen all the edges.\r\n";
    cin >> iDots >> iEdges;
    adjacency = new bool*[iDots];
    for (int iRow = 0;iRow != iDots;++iRow){
        *(adjacency + iRow) = new bool[iDots];
        for (int iColumn = 0;iColumn != iDots;++iColumn)
            *(*(adjacency + iRow) + iColumn) = 0;
    }
    while (iEdges-- != 0){
        int iDot1,iDot2;
        cin >> iDot1 >> iDot2;
        *(*(adjacency + iDot1) + iDot2) = 1;                                                  //directed edges
    }
}
Diagraph::~Diagraph(){
    if (mixed_list != NULL) delete [] mixed_list;
    for (int iCnt = 0;iCnt != iDots;++iCnt)
        delete [] *(adjacency + iCnt);
    delete [] adjacency;
}
void Diagraph::Mixed_Lists()                                                                  //migrate the data from array to mixed list
{
    mixed_list = new EdgeM*[iDots];
    for (int iRow = 0;iRow != iDots;++iRow){
        *(mixed_list + iRow) = NULL;
        EdgeM *peCurr = NULL;
        for (int iColumn = 0;iColumn != iDots;++iColumn){
            if (*(*(adjacency + iRow) + iColumn)){
                if (!peCurr){
                    *(mixed_list + iRow) = new EdgeM(iColumn);
                    peCurr = *(mixed_list + iRow);
                }
                else{
                    peCurr->next_edge = new EdgeM(iColumn);
                    peCurr = peCurr->next_edge;
                }
            }
        }
    }    
}
void Diagraph::recursive_DFS(int iVertex,bool *visited,List<int> &topological)                //recursive method for depth-first sort
{
    visited[iVertex] = true;                                                                  //mark current vertex as visited
    EdgeM *peCurr = *(mixed_list + iVertex);
    while (peCurr != NULL){                                                                   //traverse all adjacent vertices
        if (!visited[peCurr->iVertex]) recursive_DFS(peCurr->iVertex,visited,topological);
        peCurr = peCurr->next_edge;
    }
    topological.Insert(0,iVertex);                                                            //insert this vertex to the topological sequence
}
void Diagraph::DFSort(List<int> &topological)                                                 //depth-first sort
{
    bool *visited = new bool[iDots];
    for (int iCnt = 0;iCnt != iDots;++iCnt)                                                   //initialize, as none has been visited
        visited[iCnt] = false;
    for (int iCnt = 0;iCnt != iDots;++iCnt)                                                   //traverse every vertex
        if (!visited[iCnt]) recursive_DFS(iCnt,visited,topological);
    delete [] visited;
}
void Diagraph::BFSort(List<int> &topological)                                                 //breadth-first sort
{
    int *predecessor_cnt = new int[iDots];                                                    //array to keep in-degree for vertices
    for (int iCnt = 0;iCnt != iDots;++iCnt){                                                  //initialize in-degrees
        predecessor_cnt[iCnt] = 0;
        EdgeM *peCurr = *(mixed_list + iCnt);
        while(peCurr != NULL){
            ++predecessor_cnt[peCurr->iVertex];
            peCurr = peCurr->next_edge;
        }
    }
    MyQueue<int> mqVertices;                                                                  //queue to keep vertices with 0 in-degree
    for (int iCnt = 0;iCnt != iDots;++iCnt)
        if (0 == predecessor_cnt[iCnt]) mqVertices.append(iCnt);
    while (!mqVertices.empty()){
        int ivRev = 0;
        mqVertices.retrieve(ivRev);
        topological.Insert(topological.size(),ivRev);                                         //append the retrieved vertex to the result sequence
        EdgeM *peCurr = *(mixed_list + ivRev);
        while(peCurr != NULL){                                                                //reduce the in-degree of adjacent vertices
            if(0 == --predecessor_cnt[peCurr->iVertex]) mqVertices.append(peCurr->iVertex);
            peCurr = peCurr->next_edge;
        }
        mqVertices.serve();                                                                   //serve at last
    }
    delete [] predecessor_cnt;
}