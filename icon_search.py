#acad icon search, remove & rename

import os, sys, re, string, p4cmd

icon_suffix = r'.ico'
rc_suffix = r'.rc'
iconinrc = R'^\w+\s+\w+\s+DISCARDABLE\s+"\w+.ico"$'               #regex
iconinrc2 = R'^[\w\s]+["/]\w*\\{0,2}\w+\.ico"?$'                                #ignored some .icos
iconinrc3 = R'^[\w\s\+-]+["/]\w*\\{0,2}[\w\s\+-]+\.ico"?$'
iconinrc4 = R'.+\.ico"?.*$'
darkicon = R'^[\w\s]+"\w+_dark.ico"$'                       
srcicon_path = R'C:\Users\t_qix\Documents\Autodesk\ICON\icon'
macicon_path = 'C:\\Users\\t_qix\\Documents\\KastleU\\develop\\global\\rc\\osx'
install_path = 'C:\\Users\\t_qix\\Documents\\Autodesk\\ICON\\BRE\\G081.G.000.acad.swl.x64'
result_path = R'C:\Users\Public\adsk'
develop_path = R'D:\Depot\Branches\U\develop'
component_path = R'D:\Depot\Branches\U\components'
acadbtncore_path = R'D:\Depot\Branches\U\develop\global\rc\Core'
acadbtnrc_path = R'D:\Depot\Branches\U\develop\global\rc\Core\acadbtn.rc'
acadbtnltrc_path = R'D:\Depot\Branches\U\develop\global\rc\Core\acadltbtn.rc'


total_icodecl = [] #all .ico in rc files
icon_map = dict()  #entire icon map
union_map = dict() #union of entire map and all the .ico files
unico = []       #un-used icon list
src_icons = []   #all icos

nonw = dict()

rcfile_paths = []  #store all valid rcfile paths

def ListAllIcons(icons_path, delete, outfile = None):
    if os.path.isdir(icons_path):
        file_list = os.listdir(icons_path)
        file_list = [s.lower() for s in file_list]#lower
        tar_list = filter(IsAnIcon, file_list)
        if len(tar_list) > 0:
            #print icons_path, len(tar_list), 'icons'
            if delete == True:
                for icon in tar_list:
                    ModifyIconFile(icons_path, icon, outfile)
        dir_list = filter(lambda x: \
                          (len(re.findall(R'\w+\.\w{1,5}', x)) == 0), file_list)
        for directory in dir_list:
            tar_list += ListAllIcons(icons_path + '\\' + directory, delete, outfile)
        if delete == True:
            return []
        else:
            return tar_list
    else:
        return []

def ExtractRC(rc_path):
    global total_icodecl
    if os.path.isdir(rc_path):
        file_list = os.listdir(rc_path)
        file_list = [s.lower() for s in file_list]#lower
        tar_list = filter(IsAnRC, file_list)
        cnt = len(file_list)
        if len(tar_list) > 0:
            #print '*', rc_path, len(tar_list), 'ico decls'
            for rcfile in tar_list:
                total_icodecl += ReadIconsFromRC(rc_path + '\\' + rcfile)
        dir_list = filter(lambda x: \
                          (os.path.isdir(rc_path + '\\' + x)), file_list)
        for directory in dir_list:
            ExtractRC(rc_path + '\\' + directory)
    else:
        if IsAnRC(rc_path):
            total_icodecl += ReadIconsFromRC(rc_path)

def IsAnIcon(name):
    return (string.find(name, icon_suffix) != -1)

def IsAnRC(name):
    return (string.find(name, rc_suffix) != -1)

def ReadIconsFromRC(rcfilename):
    global rcfile_paths
    infile = open(rcfilename)
    try:
        #ico_map = dict()
        text = infile.read()

        ico_pat = re.compile(iconinrc4, re.I|re.M)
        ico_mat = ico_pat.findall(text)

        #if rcfilename.find(r'version.rc') != -1:
        #    print rcfilename, len(ico_mat)
        
        if len(ico_mat) > 0:
            #collect ico in rc
            #print '*', rcfilename, len(ico_mat), 'ico s'
            rcfile_list.append(rcfilename);
        
    finally:
        infile.close()
        return ico_mat

def Add2Map(ico_entry, icon_map):
    #global icon_map
    global nonw
    first_ns = re.search(R'\S',ico_entry).start()
    first_space = re.search(R'\s',ico_entry[first_ns:]).start()

    if ico_entry[first_ns:first_ns+2] == '//':
        return
    
    ico_pos = re.search(R'\.ico', ico_entry,re.I).start()
    last_nonchar = 0
    for x in re.finditer(R'["\\/]\w',ico_entry[first_ns:ico_pos]):
        last_nonchar = x.start() + 1
    iconame = ico_entry[first_ns + last_nonchar:ico_pos + 4]

    
    nonwc = ico_entry[last_nonchar - 1:last_nonchar]
    if nonwc == r'+' or  nonwc == r'-' or  nonwc == r' ' or nonwc == r'\t' or nonwc == r'E':
        print ico_entry,'#',iconame
    if nonw.has_key(nonwc):
        nonw[nonwc] += 1
    else:
        nonw[nonwc] = 1
    
    #first_quote = re.search(R'"',ico_entry).start() + 1
    rcname = ico_entry[first_ns:first_ns + first_space]
    #iconame = ico_entry[first_quote:-1]
    iconame = iconame.lower()              #lower
    if icon_map.has_key(iconame) == False:
        icon_map[iconame] = rcname
    '''
    else:
        print 'dup', iconame+':'+rcname,'and',icon_map[iconame], '\r\n'
    '''
    
def CompareIcons(srcico_list):
    global icon_map, union_map
    unuse_ico_list = [ico for ico in srcico_list if (icon_map.has_key(ico) == False)]
    union_map = {k:v for k,v in icon_map.iteritems() if srcico_list.__contains__(k) == False}
    return unuse_ico_list
    
def WriteIcons(result_path, icons):
    outfile = open(result_path,'w')
    try:
        outfile.write(str(len(icons))+' values\r\n')
        for icon in icons:
            outfile.write(icon+'\r\n')
        #outfile.writelines(icons)
    finally:
        outfile.close()

def WriteMap(result_path, iconmap):
    outfile = open(result_path,'w')
    try:
        outfile.write(str(len(iconmap))+' values\r\n')
        siconmap = SortDict(iconmap, 'v')
        for k in siconmap:
            outfile.write(k[1]+' : '+k[0]+'\r\n')
        #outfile.writelines(icons)
    finally:
        outfile.close()

def SortDict(d, korv):
    if korv == 'k':
        return sorted(d.iteritems(), key=lambda d:d[0], reverse = False )
    elif korv == 'v':
        return sorted(d.iteritems(), key=lambda d:d[1], reverse = False )
        
#delete un-used file, '\w+_dark.ico' -> 'dark_\w+.ico'
def ModifyIconFile(full_path, file_name, outfile):
    global unico
    if unico.__contains__(file_name):
        #os.remove(full_path + '\\' + file_name)
        outfile.write(full_path + '\\' + file_name + '\r\n')
        p4cmd.P4Del(full_path + '\\' + file_name)
        #call p4 cmd here
    else:
        pat = re.compile('\w+\.ico',re.I)
        newname = pat.sub(simple_repl, file_name)
        #os.rename(full_path + '\\' + file_name, full_path + '\\' + newname)
        #p4cmd.P4Rename(full_path + '\\' + file_name, full_path + '\\' + newname)
    
def ModifyNameinRC(rcfilename):
    print rcfilename
    outfile = open(rcfilename,'r+')
    try:
        text = outfile.read()
        newtext = SubIcoInRC(text)
        outfile.truncate(0)
        outfile.seek(0)
        outfile.write(newtext)
    finally:
        outfile.close()

def SubIcoInRC(alltext):
    pat = re.compile(darkicon, re.I|re.M);
    newtext = pat.sub(repla_dark, alltext)
    return newtext

def repla_dark(mat):
    global src_icons
    cut = mat.group()#
    ico_start = re.search(R'"',cut).start()                  #should not search dark, but maybe okay here
    cutico = cut[ico_start + 1:-1]
    cutico = cutico.lower()                       #lower
    ul = cutico.find('_dark.')
    newname = 'dark_' + cutico[1:ul] + '.ico'
    if src_icons.__contains__(cutico):
        os.rename(srcicon_path + '\\' \
                            + cutico, srcicon_path + '\\' + newname)
    return cut[0:ico_start] + '"dark_' + cutico[1:ul] + '.ico"'
    '''only for dark now
    else:
        pt = cut.find('.ico')
        return cut[0:ico_start] + '"light_' + cutico[1:pt] + '.ico"'
    '''
def simple_repl(mat):
    cut = mat.group()
    ul = cut.find('_dark.')
    if ul != -1:
        return 'dark_' + cut[0:ul] + '.ico'
    else:
        return cut
'''
    else:
        pt = cut.find('.ico')
        return 'light_' + cut[0:pt] + '.ico'
'''
def DelUnUsed():
    global unico, icon_map, src_icons
    outfile = open(result_path + '\\unused_icons.txt','w')
    try:
        src_icons = ListAllIcons(component_path, False)
        src_icons += ListAllIcons(develop_path, False)

        #src_icons = ListAllIcons(srcicon_path, False)
        
        WriteIcons(result_path + '\\src_icons.txt', src_icons)

        #check all the rc in develop and component
        ExtractRC(develop_path)
        ExtractRC(component_path)

        print "\r\n got here!\r\n"

        for line in total_icodecl:
            Add2Map(line, icon_map)

        unico = CompareIcons(src_icons)
        #WriteIcons(result_path + '\\unused_icons.txt', unico)
        WriteMap(result_path + '\\all_map.txt', icon_map)
        WriteMap(result_path + '\\union_map.txt', union_map)

        print '--unused', len(unico)
    
        #1 delete unused icons
        ListAllIcons(develop_path, True, outfile)
        ListAllIcons(component_path, True, outfile)
    
    finally:
        outfile.close()
    

def RenameDarks():
    global unico, icon_map, src_icons, total_icodecl
    btnrcicos = ReadIconsFromRC(acadbtncore_path + r'acadbtn.rc')
    btnltrcicos = ReadIconsFromRC(acadbtncore_path + r'acadltbtn.rc')

    darkbtndict = dict()

    for line in btnrcicos:
        Add2Map(line, darkbtndict)
    for line in btnltrcicos:
        Add2Map(line, darkbtndict)

    
    
    pass

def iconMain():
    global unico, icon_map, src_icons, total_icodecl,    nonw

    try:
        p4cmd.P4Init()
    
        #DelUnUsed()
        
        #print nonw

        btnrcicos = ReadIconsFromRC(acadbtnrc_path)
        btnltrcicos = ReadIconsFromRC(acadbtnltrc_path)
        
        btndict = dict()
        btnltdict = dict()

        for line in btnrcicos:
            Add2Map(line, btndict)
        for line in btnltrcicos:
            Add2Map(line, btndict)

        WriteMap(result_path + '\\darkbtn.txt', {k[0]:k[1] for k in btndict.items() if k[0].find(r'_dark') != -1})
        WriteMap(result_path + '\\nondarkbtn.txt', {k[0]:k[1] for k in btndict.items() if k[0].find(r'_dark') == -1})

        #2 btnrc and btnltrc
    finally:
        p4cmd.P4Close()
    '''
        btnrcicos = ReadIconsFromRC(acadbtnrc_path)
        btnltrcicos = ReadIconsFromRC(acadbtnltrc_path)
        
        btndict = dict()
        btnltdict = dict()

        for line in btnrcicos:
            Add2Map(line, btndict)
        for line in btnltrcicos:
            Add2Map(line, btnltdict)
        

        WriteMap(result_path + '\\btnrc.txt', btndict)
        WriteMap(result_path + '\\btnltrc.txt', btnltdict)

        #notinarcico = {k[0]:k[1] for k in btnltdict.items() if \
        #               btndict.has_key(k[0]) == False}
        
        
        #WriteMap(result_path + '\\btndark.txt', notinarcico)
        WriteIcons(result_path + '\\btndark.txt', notinarcico)

    
 
    ModifyNameinRC(acadbtnrc_path)
    

    for rcfile in rcfile_paths:
        print rcfile
        ModifyNameinRC(rcfile)

        

        test = ReadIconsFromRC(r'U:\develop\global\rc\Core\acadbtn_light.rc')
        
        for ico_entry in test:
            print '--', ico_entry
            first_ns = re.search(R'\S',ico_entry).start()
            first_space = re.search(R'\s',ico_entry[first_ns:]).start()
            
            ico_pos = re.search(R'\.ico', ico_entry,re.I).start()
            last_nonchar = 0
            for x in re.finditer(R'\W\w',ico_entry[first_ns:ico_pos]):
                last_nonchar = x.start() + 1
            
            
            iconame = ico_entry[first_ns + last_nonchar:ico_pos + 4]
            
            #first_quote = re.search(R'"',ico_entry).start() + 1
            rcname = ico_entry[first_ns:first_ns + first_space]
            #iconame = ico_entry[first_quote:-1]
            iconame = iconame.lower()
            print '^^', iconame, '#',rcname

    '''
