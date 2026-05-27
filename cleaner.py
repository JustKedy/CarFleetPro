import os
import re

bad_words = ["çünkü", "dolayı", "bu yüzden", "için ", "için.", "not:", "şimdilik", "todo", "bunun için", "nedeniyle", "basit olması açısından", "test verilerini", "yaptık", "ettik", "olarak kullanıyoruz", "diye ", "hata fırlat", "geçici", "gerekiyor", "gerekli", "amacıyla", "yapacağız", "yapıyoruz"]
keep_patterns = ["GET ", "POST ", "PUT ", "DELETE ", "=====", "///"]

def process_file(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        lines = f.readlines()
        
    new_lines = []
    changed = False
    
    for line in lines:
        stripped = line.strip()
        
        if stripped.startswith("//") or stripped.startswith("<!--"):
            # Check if we should explicitly keep it
            should_keep = False
            for kp in keep_patterns:
                if kp in line:
                    should_keep = True
                    break
            
            if not should_keep:
                lower_line = line.lower()
                has_bad = False
                for bw in bad_words:
                    if bw in lower_line:
                        has_bad = True
                        break
                        
                if has_bad:
                    changed = True
                    continue
                    
        new_lines.append(line)
        
    if changed:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.writelines(new_lines)
        print(f"Cleaned: {filepath}")

for root, _, files in os.walk("."):
    for file in files:
        if file.endswith(".cs") or file.endswith(".xaml"):
            process_file(os.path.join(root, file))
