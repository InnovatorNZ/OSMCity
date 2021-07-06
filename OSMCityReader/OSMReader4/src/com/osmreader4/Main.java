package com.osmreader4;

import org.w3c.dom.*;
import org.xml.sax.SAXException;

import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;
import java.io.File;
import java.io.IOException;
import java.io.PrintWriter;
import java.util.TreeSet;

class NODE implements Comparable<NODE> {
    private long id;
    double lat, lon;

    NODE(String id, String lat, String lon) {
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
    private static final boolean strict = true;             //严格筛选，将过滤footway和path
    private static TreeSet<NODE> nodeTs = new TreeSet<>();

    public static void main(String[] args) {
        DocumentBuilderFactory dbf = DocumentBuilderFactory.newInstance();
        try {
            File fp = new File("osm.txt");
            PrintWriter pw = new PrintWriter(fp);
            DocumentBuilder db = dbf.newDocumentBuilder();
            Document document = db.parse("xml.osm");
            NodeList waylist = document.getElementsByTagName("way");
            NodeList nodelist = document.getElementsByTagName("node");
            NodeList relationlist = document.getElementsByTagName("relation");
            NodeList boundlist = document.getElementsByTagName("bounds");
            //区域基本信息
            String minlat = "", minlon = "", maxlat = "", maxlon = "";
            for (int j = 0; j < boundlist.item(0).getAttributes().getLength(); j++) {
                String cname = boundlist.item(0).getAttributes().item(j).getNodeName();
                switch (cname) {
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
            System.out.println("区域：最小纬度 " + minlat + " 最小经度 " + minlon + " 最大维度 " + maxlat + " 最大经度 " + maxlon);
            double dlat = Double.parseDouble(maxlat) - Double.parseDouble(minlat);
            double dlon = Double.parseDouble(maxlon) - Double.parseDouble(minlon);
            int dlatm = (int) Math.floor(dlat * 111195);
            int dlonm = (int) Math.floor(dlon * Math.cos((Double.parseDouble(minlat) + Double.parseDouble(maxlat)) / 2 * Math.PI / 180) * 111195);
            System.out.println("纬度差：" + dlat + "° 约合" + dlatm + "米");
            System.out.println("经度差：" + dlon + "° 约合" + dlonm + "米");
            pw.write(minlat + " " + minlon + "\n");
            //读取所有节点并排序
            System.out.println("正在读取" + nodelist.getLength() + "个节点数据……");
            for (int i = 0; i < nodelist.getLength(); i++) {
                NamedNodeMap attr_node = nodelist.item(i).getAttributes();
                String id = "", lat = "", lon = "";
                for (int j = 0; j < attr_node.getLength(); j++) {
                    switch (attr_node.item(j).getNodeName()) {
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
                //种树节点
                NodeList childNodes = nodelist.item(i).getChildNodes();
                for (int j = 0; j < childNodes.getLength(); j++) {
                    if (childNodes.item(j).getNodeName().equals("tag")) {
                        String key = childNodes.item(j).getAttributes().item(0).getNodeValue();
                        String value = childNodes.item(j).getAttributes().item(1).getNodeValue();
                        if (key.equals("natural") && value.equals("tree")) {
                            pw.write("tree\n" + lat + " " + lon + "\n");
                        }
                    }
                }
            }
            //搜寻道路和建筑物
            for (int i = 0; i < waylist.getLength(); i++) {
                System.out.println("----------第" + (i + 1) + "条道路/建筑------------");
                Element ele = (Element) waylist.item(i);
                NodeList childNodes = ele.getChildNodes();
                //搜寻tag，以判定道路基本信息
                boolean way = false, building = false, underground = false, crossing = false, oneway = false;
                String name = "无", way_kind = "unknown", building_kind = "maybe", lanes = "-1", height = "-1", building_levels = "-1", bridge = "false", landuse = "";
                int layer = 1;
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
                            case "waterway":
                                way = true;
                                way_kind = value;
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
                                underground = true;
                                break;
                            case "bridge":
                                bridge = "true";
                                break;
                            case "landuse":
                            case "leisure":
                            case "natural":
                                if (!landuse.equals("river"))
                                    landuse = value;
                                break;
                            case "water":
                                if (value.equals("river")) landuse = value;
                                else landuse = "water";
                            case "amenity":
                                if (value.equals("fountain")) landuse = value;
                                break;
                            case "footway":
                                if (value.equals("crossing")) crossing = true;
                                break;
                            case "crossing":
                                crossing = true;
                                break;
                            case "layer":
                                try {
                                    layer = Integer.parseInt(value);
                                } catch (NumberFormatException e) {
                                    e.printStackTrace();
                                }
                                break;
                            case "oneway":
                                if (!value.equals("no")) oneway = true;
                                break;
                        }
                    }
                }
                //特殊：对于首尾闭合的pedestrian应认定为landuse(一型pedestrian)
                String start_nd = "-1", end_nd = "-2";
                for (int j = 0; j < childNodes.getLength(); j++) {
                    if (childNodes.item(j).getNodeType() == Node.ELEMENT_NODE && childNodes.item(j).getNodeName().equals("nd")) {
                        start_nd = childNodes.item(j).getAttributes().item(0).getNodeValue();
                        break;
                    }
                }
                for (int j = childNodes.getLength() - 1; j >= 0; j--) {
                    if (childNodes.item(j).getNodeType() == Node.ELEMENT_NODE && childNodes.item(j).getNodeName().equals("nd")) {
                        end_nd = childNodes.item(j).getAttributes().item(0).getNodeValue();
                        break;
                    }
                }
                if (start_nd.equals(end_nd) && way && way_kind.equals("pedestrian")) {
                    way = false;
                    way_kind = "unknown";
                    landuse = "pedestrian";
                }
                //输出&写入信息
                if (way) {
                    System.out.println("道路：是");
                    System.out.println("道路名称：" + name);
                    System.out.println("道路种类：" + way_kind);
                    System.out.println("车道数：" + lanes);
                    System.out.println("高架道：" + bridge);
                    System.out.println("地下：" + underground);
                    System.out.println("交叉：" + crossing);
                    System.out.println("第几层：" + layer);
                    System.out.println("单行道：" + oneway);
                } else if (building) {
                    int height_n, levels_n;
                    try {
                        height_n = (int) Float.parseFloat(height);
                    } catch (NumberFormatException e) {
                        height = height.replace(" m", "");
                        try {
                            height_n = (int) Float.parseFloat(height);
                        } catch (NumberFormatException e2) {
                            height_n = -1;
                        }
                    }
                    try {
                        levels_n = (int) Float.parseFloat(building_levels);
                    } catch (NumberFormatException e) {
                        levels_n = -1;
                    }
                    if (height_n == 0) height_n = -1;
                    if (levels_n == 0) levels_n = -1;
                    System.out.println("建筑物：是");
                    System.out.println("建筑物名称：" + name);
                    if (building_kind.equals("yes")) System.out.println("建筑物种类：未标注");
                    else System.out.println("建筑物种类：" + building_kind);
                    System.out.println("建筑物高度：" + height_n);
                    System.out.println("楼层数：" + levels_n);
                    pw.write("building " + building_kind + " " + height_n + " " + levels_n + "\n");
                } else if (!landuse.equals("")) {
                    System.out.println("特定用途用地：是");
                    if (!landuse.equals("pedestrian")) {
                        System.out.println("用途类型：" + landuse);
                        pw.write("landuse " + landuse + "\n");
                    } else {
                        System.out.println("用途类型：一型pedestrian");
                        pw.write("landuse pedestrian 1\n");
                    }
                } else {
                    System.out.println("（跳过）");
                    continue;
                }
                if (way_kind.equals("service") || way_kind.equals("steps") || way_kind.equals("cycleway") || underground || crossing || layer < 0) {
                    System.out.println("（跳过）");
                    continue;
                } else if (strict && way_kind.equals("footway") || way_kind.equals("path")) {
                    System.out.println("（严格筛选已启用，跳过）");
                    continue;
                }
                if (way) {
                    int lanes_n;
                    try {
                        lanes_n = Integer.parseInt(lanes);
                    } catch (NumberFormatException e) {
                        lanes_n = -1;
                    }
                    if (lanes_n == 0) lanes_n = -1;
                    if (bridge == "true" && layer == 1) bridge = "ground";
                    pw.write("way " + way_kind + " " + lanes_n + " " + bridge + " " + oneway + "\n");
                }
                //搜寻node，以判定道路/建筑物路径/途经点
                for (int j = 0; j < childNodes.getLength(); j++) {
                    if (childNodes.item(j).getNodeName().equals("nd")) {
                        NamedNodeMap attributes = childNodes.item(j).getAttributes();
                        String nodeid = attributes.item(0).getNodeValue();
                        NODE nd = nodeTs.floor(new NODE(nodeid, "0", "0"));
                        if (nd != null) {
                            System.out.println("维度：" + nd.lat);
                            System.out.println("经度：" + nd.lon);
                            pw.write(nd.lat + " " + nd.lon + "\n");
                        }
                    }
                }
            }

            //搜寻套娃闭合曲线
            for (int i = 0; i < relationlist.getLength(); i++) {
                System.out.println("----------第" + (i + 1) + "条套娃闭合曲线------------");
                Element ele = (Element) relationlist.item(i);
                NodeList childNodes = ele.getChildNodes();
                //搜寻tag
                boolean multipolygon = false, building = false, way = false, underground = false, crossing = false, oneway = false;
                String name = "无", way_kind = "unknown", building_kind = "maybe", lanes = "-1", height = "-1", building_levels = "-1", bridge = "false", landuse = "";
                int layer = 1;
                for (int j = 0; j < childNodes.getLength(); j++) {
                    if (childNodes.item(j).getNodeName().equals("tag")) {
                        String key = childNodes.item(j).getAttributes().item(0).getNodeValue();
                        String value = childNodes.item(j).getAttributes().item(1).getNodeValue();
                        switch (key) {
                            case "type":
                                if (value.equals("multipolygon"))
                                    multipolygon = true;
                                break;
                            case "name":
                                name = value;
                                break;
                            case "highway":
                                if (!value.equals("pedestrian")) {
                                    way = true;
                                    way_kind = value;
                                } else {
                                    landuse = value;
                                }
                                break;
                            case "waterway":
                                way = true;
                                way_kind = value;
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
                                underground = true;
                                break;
                            case "bridge":
                                bridge = "true";
                                break;
                            case "landuse":
                            case "leisure":
                            case "natural":
                                if (!landuse.equals("river"))
                                    landuse = value;
                                break;
                            case "water":
                                if (value.equals("river")) landuse = value;
                                else landuse = "water";
                            case "footway":
                                if (value.equals("crossing")) crossing = true;
                                break;
                            case "crossing":
                                crossing = true;
                                break;
                            case "layer":
                                try {
                                    layer = Integer.parseInt(value);
                                } catch (NumberFormatException e) {
                                    e.printStackTrace();
                                }
                                break;
                            case "oneway":
                                if (!value.equals("no")) oneway = true;
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
                    System.out.println("地下：" + underground);
                    System.out.println("交叉：" + crossing);
                    System.out.println("第几层：" + layer);
                } else if (building) {
                    int height_n, levels_n;
                    try {
                        height_n = (int) Float.parseFloat(height);
                    } catch (NumberFormatException e) {
                        height = height.replace(" m", "");
                        try {
                            height_n = (int) Float.parseFloat(height);
                        } catch (NumberFormatException e2) {
                            height_n = -1;
                        }
                    }
                    try {
                        levels_n = (int) Float.parseFloat(building_levels);
                    } catch (NumberFormatException e) {
                        levels_n = -1;
                    }
                    if (height_n == 0) height_n = -1;
                    if (levels_n == 0) levels_n = -1;

                    System.out.println("建筑物：是");
                    System.out.println("建筑物名称：" + name);
                    if (building_kind.equals("yes")) System.out.println("建筑物种类：未标注");
                    else System.out.println("建筑物种类：" + building_kind);
                    System.out.println("建筑物高度：" + height_n);
                    System.out.println("楼层数：" + levels_n);
                    pw.write("building " + building_kind + " " + height_n + " " + levels_n + "\n");
                } else if (!landuse.equals("")) {
                    System.out.println("特定用途用地：是");
                    if (!landuse.equals("pedestrian")) {
                        System.out.println("用途类型：" + landuse);
                        pw.write("landuse " + landuse + "\n");
                    } else {
                        System.out.println("用途类型：二型pedestrian");
                        pw.write("landuse pedestrian 2\n");
                    }
                } else if (multipolygon) {
                    System.out.println("（忽略该闭合曲线）");
                    continue;
                } else {
                    System.out.println("（非闭合曲线，跳过）");
                    continue;
                }
                if (way_kind.equals("service") || way_kind.equals("steps") || way_kind.equals("cycleway") || underground || crossing || layer < 0) {
                    System.out.println("（跳过）");
                    continue;
                } else if (strict && way_kind.equals("footway") || way_kind.equals("path")) {
                    System.out.println("（严格筛选已启用，跳过）");
                    continue;
                }
                if (way) {
                    int lanes_n;
                    try {
                        lanes_n = Integer.parseInt(lanes);
                    } catch (NumberFormatException e) {
                        lanes_n = -1;
                    }
                    if (lanes_n == 0) lanes_n = -1;
                    pw.write("way " + way_kind + " " + lanes_n + " " + bridge + " " + oneway + "\n");
                }
                //搜寻member way
                for (int j = 0; j < childNodes.getLength(); j++) {
                    if (childNodes.item(j).getNodeName().equals("member")) {
                        NamedNodeMap attr = childNodes.item(j).getAttributes();
                        String type = "", ref = "";
                        for (int k = 0; k < attr.getLength(); k++) {
                            String cname = attr.item(k).getNodeName();
                            String cvalue = attr.item(k).getNodeValue();
                            switch (cname) {
                                case "type":
                                    type = cvalue;
                                    break;
                                case "ref":
                                    ref = cvalue;
                                    break;
                            }
                        }
                        if (type.equals("way")) {
                            for (int k = 0; k < waylist.getLength(); k++) {
                                String cref = "";
                                for (int l = 0; l < waylist.item(k).getAttributes().getLength(); l++) {
                                    if (waylist.item(k).getAttributes().item(l).getNodeName().equals("id")) {
                                        cref = waylist.item(k).getAttributes().item(l).getNodeValue();
                                        break;
                                    }
                                }
                                if (cref.equals(ref)) {
                                    NodeList wayChildNodes = waylist.item(k).getChildNodes();
                                    //查询way的nodes
                                    for (int l = 0; l < wayChildNodes.getLength(); l++) {
                                        if (wayChildNodes.item(l).getNodeName().equals("nd")) {
                                            String nodeid = wayChildNodes.item(l).getAttributes().item(0).getNodeValue();
                                            NODE nd = nodeTs.floor(new NODE(nodeid, "0", "0"));
                                            if (nd != null) {
                                                System.out.println("维度：" + nd.lat);
                                                System.out.println("经度：" + nd.lon);
                                                pw.write(nd.lat + " " + nd.lon + "\n");
                                            }
                                        }
                                    }
                                    System.out.println("------子分割线------");
                                    pw.write("-1073741824 -1073741824\n");
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            pw.close();
        } catch (ParserConfigurationException | IOException | SAXException | NullPointerException e) {
            e.printStackTrace();
        }
    }
}
