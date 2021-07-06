package com.osmreader;

import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;

import org.w3c.dom.*;
import org.xml.sax.SAXException;
import java.io.File;
import java.io.IOException;
import java.util.Iterator;

public class Main {
    public static void main(String[] args) {
        DocumentBuilderFactory dbf = DocumentBuilderFactory.newInstance();
        try{
            DocumentBuilder db = dbf.newDocumentBuilder();
            Document document = db.parse("xml.osm");
            NodeList waylist = document.getElementsByTagName("way");
            for(int i = 0; i < waylist.getLength(); i++){
                System.out.println("--------第" + (i+1) + "条道路----------");
                Element ele = (Element) waylist.item(i);
                NodeList childNodes = ele.getChildNodes();
                for(int j = 0; j < childNodes.getLength(); j++){
                    if (childNodes.item(j).getNodeType()==Node.ELEMENT_NODE) {
                        //获取节点
                        System.out.print(childNodes.item(j).getNodeName() + ": ");
                        //获取节点值
                        try {
                            System.out.print("Value: ");
                            System.out.println(childNodes.item(j).getFirstChild().getNodeValue());
                        }catch(NullPointerException e){
                            System.out.print("None ");
                        }
                        //获取属性值
                        try{
                            System.out.print("AttributeNum: ");
                            System.out.print(childNodes.item(j).getAttributes().getLength());
                            NamedNodeMap attributes = childNodes.item(j).getAttributes();
                            System.out.print("\nAttributes: ");
                            for(int k = 0; k < childNodes.item(j).getAttributes().getLength(); k++){
                                  System.out.print(attributes.item(k).getNodeName() + "=" + attributes.item(k).getNodeValue() + " ");
                            }
                        }catch(NullPointerException e){
                            System.out.print("None ");
                        }
                        System.out.println();
                    }
                }
                System.out.println("---------------------------------");
            }
        }catch (ParserConfigurationException | IOException | SAXException e){
            e.printStackTrace();
        }
    }
}
