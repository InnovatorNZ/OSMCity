package com.osmreader2;

import org.w3c.dom.*;
import org.xml.sax.SAXException;
import sun.reflect.generics.tree.Tree;

import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;
import java.io.IOException;
import java.util.TreeSet;

class NODE implements Comparable<NODE> {
    public long id;
    public double lat, lon;
    public NODE(String id, String lat, String lon){
        this.id = Long.parseLong(id);
        this.lat = Double.parseDouble(lat);
        this.lon = Double.parseDouble(lon);
    }
    @Override
    public int compareTo(NODE o) {
        return Long.compare(this.id, o.id);
    }
}

public class Main {
    private static TreeSet<NODE> nodeTs = new TreeSet<>();
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
            double dlat = Double.parseDouble(maxlat) - Double.parseDouble(minlat);
            double dlon = Double.parseDouble(maxlon) - Double.parseDouble(minlon);
            int dlatm = (int)Math.floor(dlat * 111195);
            int dlonm = (int)Math.floor(dlon * Math.cos((Double.parseDouble(minlat) + Double.parseDouble(maxlat))/2 * Math.PI / 180) * 111195);
            System.out.println("纬度差："+dlat+"° 约合"+dlatm+"米");
            System.out.println("经度差："+dlon+"° 约合"+dlonm+"米");
            //读取所有节点并排序
            System.out.println("正在读取节点数据……");
            for(int i = 0; i < nodelist.getLength(); i++){
                NamedNodeMap attr_node = nodelist.item(i).getAttributes();
                String id="", lat="", lon="";
                for (int j = 0; j < attr_node.getLength(); j++) {
                    switch(attr_node.item(j).getNodeName()){
                        case "id":
                            id = attr_node.item(j).getNodeValue();
                            break;
                        case "lat":
                            lat = attr_node.item(j).getNodeValue();
                            break;
                        case "lon":
                            lon = attr_node.item(j).getNodeValue();
                            break;
                    }
                }
                nodeTs.add(new NODE(id, lat, lon));
            }
            //搜寻道路和建筑物
            for (int i = 0; i < waylist.getLength(); i++) {
                System.out.println("----------第" + (i + 1) + "条道路/建筑------------");
                Element ele = (Element) waylist.item(i);
                NodeList childNodes = ele.getChildNodes();
                //搜寻tag，以判定道路基本信息
                boolean way = false, building = false;
                String name = "无", way_kind = "未标注", building_kind = "", lanes="未标注", height="未标注", building_levels="未标注", bridge="否", landuse="", underground="否";
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
                            case "height":
                                height = value;
                                break;
                            case "lanes":
                                lanes = value;
                                break;
                            case "building:levels":
                                building_levels = value;
                                break;
                            case "building:levels:underground":
                                underground = "是";
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
                    System.out.println("地下："+underground);
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
                        NODE nd = nodeTs.floor(new NODE(nodeid, "0", "0"));
                        System.out.println("维度：" + nd.lat);
                        System.out.println("经度：" + nd.lon);
                    }
                }
            }
        } catch (ParserConfigurationException | IOException | SAXException | NullPointerException e) {
            e.printStackTrace();
        }
    }
}
