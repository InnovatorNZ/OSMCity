package com.osmreader2;

import org.w3c.dom.*;
import org.xml.sax.SAXException;

import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;
import java.io.IOException;

public class Main {

    public static void main(String[] args) {
        DocumentBuilderFactory dbf = DocumentBuilderFactory.newInstance();
        try {
            DocumentBuilder db = dbf.newDocumentBuilder();
            Document document = db.parse("xml.osm");
            NodeList waylist = document.getElementsByTagName("way");
            NodeList nodelist = document.getElementsByTagName("node");
            NodeList boundlist = document.getElementsByTagName("bounds");
            //区域基本信息
            String minlat="", minlon="", maxlat="", maxlon="";
            for(int j=0; j<boundlist.item(0).getAttributes().getLength(); j++){
                String cname = boundlist.item(0).getAttributes().item(j).getNodeName();
                switch(cname){
                    case "minlat":
                        minlat = boundlist.item(0).getAttributes().item(j).getNodeValue();
                        break;
                    case "maxlat":
                        maxlat = boundlist.item(0).getAttributes().item(j).getNodeValue();
                        break;
                    case "minlon":
                        minlon = boundlist.item(0).getAttributes().item(j).getNodeValue();
                        break;
                    case "maxlon":
                        maxlon = boundlist.item(0).getAttributes().item(j).getNodeValue();
                        break;
                }
            }
            System.out.println("区域：最小纬度 "+minlat+" 最小经度 "+minlon+" 最大维度 "+maxlat+" 最大经度 "+maxlon);
            //搜寻道路和建筑物
            for (int i = 0; i < waylist.getLength(); i++) {
                System.out.println("----------第" + (i + 1) + "条道路/建筑------------");
                Element ele = (Element) waylist.item(i);
                NodeList childNodes = ele.getChildNodes();
                //搜寻tag，以判定道路基本信息
                boolean way = false, building = false;
                String name = "无", way_kind = "未标注", building_kind = "", lanes="未标注", height="未标注", building_levels="未标注", bridge="否", landuse="";
                for (int j = 0; j < childNodes.getLength(); j++) {
                    if (childNodes.item(j).getNodeType() == Node.ELEMENT_NODE && childNodes.item(j).getNodeName().equals("tag")) {
                        String key = childNodes.item(j).getAttributes().item(0).getNodeValue();
                        String value = childNodes.item(j).getAttributes().item(1).getNodeValue();
                        switch (key) {
                            case "name":
                                name = value;
                                break;
                            case "highway":
                                way = true;
                                way_kind = value;
                                break;
                            case "building":
                                building = true;
                                building_kind = value;
                                break;
                            case "lanes":
                                lanes = value;
                                break;
                            case "building:levels":
                                building_levels = value;
                                break;
                            case "bridge":
                                bridge = "是";
                                break;
                            case "landuse":
                                landuse = value;
                                break;
                        }
                    }
                }
                if (way) {
                    System.out.println("道路：是");
                    System.out.println("道路名称：" + name);
                    System.out.println("道路种类：" + way_kind);
                    System.out.println("车道数：" + lanes);
                    System.out.println("高架道：" + bridge);
                } else if (building) {
                    System.out.println("建筑物：是");
                    System.out.println("建筑物名称：" + name);
                    if (building_kind.equals("yes")) System.out.println("建筑物种类：未标注");
                    else System.out.println("建筑物种类：" + building_kind);
                    System.out.println("建筑物高度：" + height);
                    System.out.println("楼层数：" + building_levels);
                } else if(!landuse.equals("")){
                    System.out.println("特定用途用地：是");
                    System.out.println("用途类型：" + landuse);
                } else {
                    System.out.println("（跳过）");
                    continue;
                }
                //搜寻node，以判定道路/建筑物路径/途经点
                if(way_kind.equals("footway") || way_kind.equals("service")){
                    System.out.println("（道路过小自动跳过）");
                    continue;
                }
                for (int j = 0; j < childNodes.getLength(); j++) {
                    if (childNodes.item(j).getNodeName().equals("nd")) {
                        NamedNodeMap attributes = childNodes.item(j).getAttributes();
                        String nodeid = attributes.item(0).getNodeValue();
                        for (int k = 0; k < nodelist.getLength(); k++) {
                            NamedNodeMap attr_node = nodelist.item(k).getAttributes();
                            for (int l = 0; l < attr_node.getLength(); l++) {
                                if (attr_node.item(l).getNodeName().equals("id") && attr_node.item(l).getNodeValue().equals(nodeid)) {
                                    for (int m = 0; m < attr_node.getLength(); m++) {
                                        if (attr_node.item(m).getNodeName().equals("lat"))
                                            System.out.println("维度：" + attr_node.item(m).getNodeValue());
                                        else if (attr_node.item(m).getNodeName().equals("lon"))
                                            System.out.println("经度：" + attr_node.item(m).getNodeValue());
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        } catch (ParserConfigurationException | IOException | SAXException | NullPointerException e) {
            e.printStackTrace();
        }
    }
}
