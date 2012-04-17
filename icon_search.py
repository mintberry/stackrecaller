#acad icon search, remove & rename

import os, sys, re, string

icon_suffix = r'.ico'
rc_suffix = r'.rc'
iconinrc = R'^\w+\s+\w+\s+DISCARDABLE\s+"\w+.ico"$'               #regex
iconinrc2 = R'^[\w\s]+"\w+.ico"$'
srcicon_path = 'C:\\Users\\t_qix\\Documents\\KastleU\\develop\\global\\rc\\icon'
macicon_path = 'C:\\Users\\t_qix\\Documents\\KastleU\\develop\\global\\rc\\osx'
install_path = 'C:\\Users\\t_qix\\Documents\\Autodesk\\ICON\\BRE\\G081.G.000.acad.swl.x64'
result_path = 'C:\\Users\\t_qix\\Documents\\Autodesk\\ICON'
develop_path = R'C:\Users\t_qix\Documents\KastleU\develop'
component_path = R'C:\Users\t_qix\Documents\KastleU\components'

total_icodecl = []
icon_map = dict()
union_map = dict()

def ListAllIcons(icons_path):
    if os.path.isdir(icons_path):
        file_list = os.listdir(icons_path)
        tar_list = filter(IsAnIcon, file_list)
        if len(tar_list) > 0:
            print icons_path, len(tar_list), 'icons'
        dir_list = filter(lambda x: \
                          (len(re.findall(R'\w+\.\w{1,5}', x)) == 0), file_list)
        for directory in dir_list:
            tar_list += ListAllIcons(icons_path + '\\' + directory)
        return tar_list
    else:
        return []

def ExtractRC(rc_path):
    global total_icodecl
    if os.path.isdir(rc_path):
        file_list = os.listdir(rc_path)
        tar_list = filter(IsAnRC, file_list)
        cnt = len(file_list)
        if len(tar_list) > 0:
            #print '*', rc_path, len(tar_list), 'ico decls'
            for rcfile in tar_list:
                total_icodecl += ReadIconsFromRC(rc_path + '\\' + rcfile)
        dir_list = filter(lambda x: \
                          (len(re.findall(R'\w+\.\w{1,5}', x)) == 0), file_list)
        for directory in dir_list:
            ExtractRC(rc_path + '\\' + directory)

def IsAnIcon(name):
    return (string.find(name, icon_suffix) != -1)

def IsAnRC(name):
    return (string.find(name, rc_suffix) != -1)

def ReadIconsFromRC(rcfilename):
    infile = open(rcfilename)
    try:
        #ico_map = dict()
        text = infile.read()
        ico_pat = re.compile(iconinrc2, re.I|re.M)
        ico_mat = ico_pat.findall(text)
        
        if len(ico_mat) > 0:
            #collect ico in rc
            print '*', rcfilename, len(ico_mat), 'ico decls'
    finally:
        infile.close()
        return ico_mat

def Add2Map(ico_entry):
    global icon_map
    first_space = re.search(R'\s',ico_entry).start()
    first_quote = re.search(R'"',ico_entry).start() + 1
    rcname = ico_entry[0:first_space]
    iconame = ico_entry[first_quote:-1]
    if icon_map.has_key(iconame) == False:
        icon_map[iconame] = rcname
    else:
        print 'dup', iconame+':'+rcname,'and',icon_map[iconame], '\r\n'

def CompareIcons(srcico_list):
    global icon_map, union_map
    unuse_ico_list = [ico for ico in srcico_list if (icon_map.has_key(ico) == False)]
    union_map = {k:v for k,v in icon_map.iteritems() if srcico_list.__contains__(k)}
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
        for k in iconmap:
            outfile.write(iconmap[k]+' : '+k+'\r\n')
        #outfile.writelines(icons)
    finally:
        outfile.close()

def iconMain():
    
    src_icons = \
              ListAllIcons(srcicon_path)
    #src_icons += ListAllIcons(component_path)
    #install_icons = \
    #              ListAllIcons(install_path)
    WriteIcons(result_path + '\\src.txt', src_icons)

    ExtractRC(develop_path)
    ExtractRC(component_path)

    for line in total_icodecl:
        Add2Map(line)

    unuse_icons = CompareIcons(src_icons)
    WriteIcons(result_path + '\\unico2.txt', unuse_icons)
    WriteMap(result_path + '\\allmap2.txt', icon_map)
    WriteMap(result_path + '\\unimap2.txt', union_map)

    #WriteIcons(result_path + '\\icodecl.txt', total_icodecl)
    
    #WriteIcons(result_path + '\\install.txt', install_icons)

    
